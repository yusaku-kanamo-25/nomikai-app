using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace FunctionbeerAPI
{
    public static class Function1
    {
        /// <summary>
        /// データベース接続文字列を取得する
        /// 優先順位: 1. 環境変数 2. Key Vault (オプション)
        /// </summary>
        private static async Task<string> GetConnectionStringAsync()
        {
            // 優先順位 1: 環境変数から取得
            string connectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
            
            // 優先順位 2: Key Vault から取得 (オプショナル)
            string kvUri = Environment.GetEnvironmentVariable("KeyVaultUri");
            if (!string.IsNullOrEmpty(kvUri))
            {
                try
                {
                    var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
                    KeyVaultSecret secret = await client.GetSecretAsync("DatabaseConnectionString");
                    return secret.Value;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"環境変数 'DatabaseConnectionString' が見つからず、Key Vault 取得に失敗: {ex.Message}", ex);
                }
            }
            
            // 接続文字列が見つからない
            throw new InvalidOperationException("データベース接続文字列が見つかりません。'DatabaseConnectionString' 環境変数を設定するか、Key Vault を設定してください。");
        }

        // リクエストデータの型定義クラス
        public class CalculateRequest
        {
            public decimal TotalAmount { get; set; }
            public int NumberOfParticipants { get; set; }
            public int EventID { get; set; }
            public int ParticipantID { get; set; }
        }

        [FunctionName("Calculate")]
        public static async Task<IActionResult> Calculate(
     [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "calculate")] HttpRequest req,
     ILogger log)
        {
            log.LogInformation("Calculate function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CalculateRequest data = JsonConvert.DeserializeObject<CalculateRequest>(requestBody);

            if (data.NumberOfParticipants <= 0)
            {
                return CreateCorsResponse(new BadRequestObjectResult("参加人数は0より大きい必要があります。"), req.HttpContext.Response);
            }

            decimal amountPerParticipant = data.TotalAmount / data.NumberOfParticipants;

            try
            {
                string connectionString = await GetConnectionStringAsync();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Nomikai テーブルの ID 存在確認
                    var checkQuery = "SELECT COUNT(*) FROM Nomikai WHERE ID = @ID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ID", data.EventID);  // @ID に CalculateRequest からの値を設定
                        int count = (int)await checkCmd.ExecuteScalarAsync();
                        if (count == 0)
                        {
                            return new BadRequestObjectResult("EventID does not exist.");
                        }
                    }

                    // Nomikai テーブルへの UPDATE 処理
                    var updateQuery = "UPDATE Nomikai SET amount = @Amount WHERE ID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventID", data.EventID);
                        cmd.Parameters.AddWithValue("@Amount", amountPerParticipant);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"データベース操作エラー: {ex.Message}");
                return CreateCorsResponse(new StatusCodeResult(StatusCodes.Status500InternalServerError), req.HttpContext.Response);
            }

            var result = new
            {
                TotalAmount = data.TotalAmount,
                NumberOfParticipants = data.NumberOfParticipants,
                AmountPerParticipant = amountPerParticipant
            };

            return CreateCorsResponse(new OkObjectResult(result), req.HttpContext.Response);
        }

        [FunctionName("GetHistory")]
        public static async Task<IActionResult> GetHistory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "history")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetHistory function processed a request.");

            var history = new List<object>();

            try
            {
                string connectionString = await GetConnectionStringAsync();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var query = "SELECT * FROM Payments";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            history.Add(new
                            {
                                PaymentID = reader["PaymentID"],
                                EventID = reader["EventID"],
                                ParticipantID = reader["ParticipantID"],
                                AmountPaid = reader["AmountPaid"]
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error during database operation: {ex.Message}");
                return CreateCorsResponse(new StatusCodeResult(StatusCodes.Status500InternalServerError), req.HttpContext.Response);
            }

            return CreateCorsResponse(new OkObjectResult(history), req.HttpContext.Response);
        }

        [FunctionName("SaveHistory")]
        public static async Task<IActionResult> SaveHistory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "history")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SaveHistory function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            try
            {
                string connectionString = await GetConnectionStringAsync();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var query = "INSERT INTO Events (EventDate, TotalAmount) VALUES (@EventDate, @TotalAmount)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventDate", data?.eventDate);
                        cmd.Parameters.AddWithValue("@TotalAmount", data?.totalAmount);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error during database operation: {ex.Message}");
                return CreateCorsResponse(new StatusCodeResult(StatusCodes.Status500InternalServerError), req.HttpContext.Response);
            }

            var response = new
            {
                Message = "Data has been saved successfully."
            };

            return CreateCorsResponse(new OkObjectResult(response), req.HttpContext.Response);
        }

        public class NomikaiEventRequest
        {
            public DateTime EventDate { get; set; }
            public string EventName { get; set; }
            public string Participants { get; set; }
            public decimal Amount { get; set; }
            public bool PaymentFlag { get; set; }
        }

        [FunctionName("SaveNomikaiEvent")]
        public static async Task<IActionResult> SaveNomikaiEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "savenomikai")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SaveNomikaiEvent function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NomikaiEventRequest data = JsonConvert.DeserializeObject<NomikaiEventRequest>(requestBody);

            log.LogInformation($"Received Amount: {data.Amount}");

            if (data.Amount <= 0)
            {
                return CreateCorsResponse(new BadRequestObjectResult(new { Message = "金額は0より大きい必要があります。" }), req.HttpContext.Response);
            }

            // 参加者をカンマで分割し、各参加者のトリミング
            var participants = data.Participants.Split(new[] { "、" }, StringSplitOptions.RemoveEmptyEntries);

            // 総額を人数で割る
            decimal amountPerParticipant = data.Amount / (decimal)participants.Length;

            try
            {
                string connectionString = await GetConnectionStringAsync();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (var participant in participants)
                    {
                        var trimmedParticipant = participant.Trim();

                        var query = "INSERT INTO Nomikai (event_date, event_name, participants, amount, payment_flag) VALUES (@EventDate, @EventName, @Participant, @Amount, @PaymentFlag)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@EventDate", data.EventDate);
                            cmd.Parameters.AddWithValue("@EventName", data.EventName);
                            cmd.Parameters.AddWithValue("@Participant", trimmedParticipant);
                            cmd.Parameters.AddWithValue("@Amount", amountPerParticipant);
                            cmd.Parameters.AddWithValue("@PaymentFlag", data.PaymentFlag);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"データベース操作エラー: {ex.Message}");
                return CreateCorsResponse(new BadRequestObjectResult(new { Message = $"データベース操作エラー: {ex.Message}" }), req.HttpContext.Response);
            }

            var response = new
            {
                Message = "飲み会イベントが全参加者分保存されました。"
            };

            return CreateCorsResponse(new OkObjectResult(response), req.HttpContext.Response);
        }

        [FunctionName("SearchNomikaiEvent")]
        public static async Task<IActionResult> SearchNomikaiEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "nomikai/search")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SearchNomikaiEvent function processed a request.");

            string eventName = req.Query["eventname"];
            string eventDate = req.Query["eventdate"];
            string name = req.Query["name"];

            if (string.IsNullOrEmpty(eventName) && string.IsNullOrEmpty(eventDate) && string.IsNullOrEmpty(name))
            {
                return CreateCorsResponse(new BadRequestObjectResult("少なくとも1つのクエリパラメータ (eventname, eventdate, または name) が必要です。"), req.HttpContext.Response);
            }

            var events = new List<object>();

            try
            {
                string connectionString = await GetConnectionStringAsync();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Nomikai WHERE 1=1"; // 基本のクエリ条件

                    if (!string.IsNullOrEmpty(eventName))
                    {
                        query += " AND event_name LIKE @EventName";
                    }

                    if (!string.IsNullOrEmpty(eventDate))
                    {
                        query += " AND event_date = @EventDate";
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        query += " AND participants LIKE @Name";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(eventName))
                        {
                            cmd.Parameters.AddWithValue("@EventName", "%" + eventName + "%");
                        }

                        if (!string.IsNullOrEmpty(eventDate))
                        {
                            cmd.Parameters.AddWithValue("@EventDate", eventDate);
                        }

                        if (!string.IsNullOrEmpty(name))
                        {
                            cmd.Parameters.AddWithValue("@Name", "%" + name + "%");
                        }

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                events.Add(new
                                {
                                    id = reader["id"],
                                    eventDate = reader["event_date"],
                                    eventName = reader["event_name"],
                                    participants = reader["participants"],
                                    amount = reader["amount"],
                                    paymentFlag = reader["payment_flag"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"データベース操作エラー: {ex.Message}");
                return CreateCorsResponse(new StatusCodeResult(StatusCodes.Status500InternalServerError), req.HttpContext.Response);
            }

            return CreateCorsResponse(new OkObjectResult(events), req.HttpContext.Response);
        }

        [FunctionName("UpdatePaymentFlags")]
        public static async Task<IActionResult> UpdatePaymentFlags(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "updatepaymentflags")] HttpRequest req,
    ILogger log)
        {
            log.LogInformation("UpdatePaymentFlags function processed a request.");

            // リクエストボディを読み込む
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updates = JsonConvert.DeserializeObject<List<PaymentFlagUpdateRequest>>(requestBody);

            if (updates == null || updates.Count == 0)
            {
                return CreateCorsResponse(new BadRequestObjectResult("更新データがありません。"), req.HttpContext.Response);
            }

            try
            {
                string connectionString = await GetConnectionStringAsync();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (var update in updates)
                    {
                        var query = "UPDATE Nomikai SET payment_flag = @PaymentFlag WHERE id = @ID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@PaymentFlag", update.PaymentFlag);
                            cmd.Parameters.AddWithValue("@ID", update.Id);

                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0)
                            {
                                log.LogWarning($"No record found for ID: {update.Id}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"データベース操作エラー: {ex.Message}");
                return CreateCorsResponse(new StatusCodeResult(StatusCodes.Status500InternalServerError), req.HttpContext.Response);
            }

            var response = new
            {
                Message = "支払いフラグが更新されました。"
            };

            return CreateCorsResponse(new OkObjectResult(response), req.HttpContext.Response);
        }

        // 支払いフラグ更新リクエストのクラス定義
        public class PaymentFlagUpdateRequest
        {
            public int Id { get; set; }
            public bool PaymentFlag { get; set; }
        }

        // CORS 設定を適用するヘルパーメソッド
        private static IActionResult CreateCorsResponse(IActionResult result, HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return result;
        }
    }
}

