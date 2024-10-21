using System;
using MySql.Data.MySqlClient;

namespace Microsoft.Samples.Kinect.WpfViewers
{
    
    internal class ServerConnected
    {
        static void Main(string[] args)
        {
            // MySQL 데이터베이스 연결 문자열 설정
            string connectionString = "server=localhost;port=3306;database=fff;uid=root;password=1234;";

            // MySQL 데이터베이스에 연결
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();

                // 데이터 삽입 쿼리
                string query = "INSERT INTO users (username, email) VALUES ('wefasfef', '123@ananan.com')";

                // 쿼리 실행을 위한 MySqlCommand 객체 생성
                MySqlCommand command = new MySqlCommand(query, connection);

                // 쿼리 실행
                command.ExecuteNonQuery();

                Console.WriteLine("데이터 삽입 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine("데이터 삽입 중 오류 발생: " + ex.Message);
            }
            finally
            {
                // 연결 닫기
                connection.Close();
            }
        }
    }
}
