using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;

namespace LacunaDNA
{
    class Program
    {

        private static string token;

        static void Main(string[] args)
        {
            /*var resposta = CriaUsuarioRestSharp();
            Console.WriteLine(resposta);*/

            for (int i = 0; i < 11; i++){

                token = GetToken();

                JToken job_values = GetJob(token);

                string job_id = job_values["id"].Value<string>();
                string job_type = job_values["type"].Value<string>();

                Console.WriteLine("The job is:");
                Console.WriteLine(job_type);

                switch (job_type)
                {
                    case "DecodeStrand":
                        string decoded_strand = DecodeStrandOpr(job_values["strandEncoded"].Value<string>());
                        SendDecodeJob(token, job_id, decoded_strand);
                        break;
                    case "EncodeStrand":
                        string encoded_strand = EncodeStrandOpr(job_values["strand"].Value<string>());
                        SendEncodeJob(token, job_id, encoded_strand);
                        break;
                    case "CheckGene":
                        bool isActivated = CheckGeneOpr(job_values["geneEncoded"].Value<string>(), job_values["strandEncoded"].Value<string>());
                        SendCheckGeneJob(token, job_id, isActivated);
                        break;
                }
            }
        }

        static string BaseToBin(string strand){
            byte[] decodedByteArray = Convert.FromBase64CharArray(strand.ToCharArray(), 0, strand.Length);
            
            string byte_string = BitConverter.ToString(decodedByteArray);
            byte_string = byte_string.Replace("-", "");

            string binarystring = String.Join(String.Empty, byte_string.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

            return binarystring;
        }

        static string DecodeStrandOpr(string StrandEncoded){
            string decoded_strand = "";
            string bin_strand = BaseToBin(StrandEncoded);

            for (int i = 0; i < bin_strand.Length-1; i+=2){
                string tmp = string.Concat(bin_strand[i], bin_strand[i + 1]);
                if (tmp.Equals("00")){
                    decoded_strand = string.Concat(decoded_strand, "A");
                } else if (tmp.Equals("01")){
                    decoded_strand = string.Concat(decoded_strand, "C");
                } else if (tmp.Equals("10")){
                    decoded_strand = string.Concat(decoded_strand, "G");
                } else {
                    decoded_strand = string.Concat(decoded_strand, "T");
                }
            }
            return decoded_strand;
        }

        public static byte[] HexStringToHex(string inputHex)
        {
            var resultantArray = new byte[inputHex.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = System.Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }

        public static string BinaryStringToHexString(string binStrand)
        {
            if (string.IsNullOrEmpty(binStrand))
                return binStrand;

            StringBuilder result = new StringBuilder(binStrand.Length / 8 + 1);

            // TODO: check all 1's or 0's... throw otherwise

            int mod4Len = binStrand.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binStrand = binStrand.PadLeft(((binStrand.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binStrand.Length; i += 8)
            {
                string eightBits = binStrand.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        static string EncodeStrandOpr(string strand) {
            string strandBin = "";

            for (int i = 0; i < strand.Length; i++)
            {
                if(strand[i] == 'A'){
                    strandBin = string.Concat(strandBin, "00");
                }
                else if(strand[i] == 'C'){
                    strandBin = string.Concat(strandBin, "01");
                }
                else if (strand[i] == 'G'){
                    strandBin = string.Concat(strandBin, "10");
                }
                else{
                    strandBin = string.Concat(strandBin, "11");
                }
            }


            string strHex = BinaryStringToHexString(strandBin);
            byte[] byteHex = HexStringToHex(strHex);
            return System.Convert.ToBase64String(byteHex);
        }

        static bool ComplementaryStrand(string strand){
            string start = strand.Substring(0, 3);
            if(start != "CAT"){
                return true;
            } else{
                return false;
            }

        }

        static string CompToTemplate(string compl_strand){
            string template_strand = "";
            foreach (char c in compl_strand){
                switch (c)
                {
                    case 'G':
                        template_strand = string.Concat(template_strand, 'C');
                        break;
                    case 'C':
                        template_strand = string.Concat(template_strand, 'G');
                        break;
                    case 'A':
                        template_strand = string.Concat(template_strand, 'T');
                        break;
                    case 'T':
                        template_strand = string.Concat(template_strand, 'A');
                        break;
                    default:
                        break;
                }
            }

            return template_strand;
        }

        /* code obtained from https://www.geeksforgeeks.org/longest-common-substring-dp-29/ */
        static int LCS(string X, string Y, int m, int n){

            int[,] LCStuff = new int[m + 1, n + 1];

            int result = 0;

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == 0 || j == 0)
                        LCStuff[i, j] = 0;
                    else if (X[i - 1] == Y[j - 1])
                    {
                        LCStuff[i, j]
                            = LCStuff[i - 1, j - 1] + 1;

                        result
                            = Math.Max(result, LCStuff[i, j]);
                    }
                    else
                        LCStuff[i, j] = 0;
                }
            }

            return result;
        }

        static bool CheckGeneOpr(string geneEncoded, string strandEncoded){

            string decoded_strand = DecodeStrandOpr(strandEncoded);
            string decoded_gene = DecodeStrandOpr(geneEncoded);

            if (ComplementaryStrand(decoded_strand)){
                decoded_strand = CompToTemplate(decoded_strand);
            }

            /* application of LCS algorithm */
            int lenght_of_substring = LCS(decoded_gene, decoded_strand, decoded_gene.Length, decoded_strand.Length);

            if (((float)lenght_of_substring / (float)decoded_gene.Length) > 0.5){
                return true;
            }else{
                return false;
            }
        }

        static string GetToken(){
            var client = new RestClient("https://gene.lacuna.cc");
            var request = new RestRequest("/api/users/login");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    username = "ThiagoMT",
                    password = "T5sDws1706"
                });
            var response = client.Post(request);
            var content = response.Content; // Raw content as string

            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            
            if (values["code"].Equals("Success")){
                return values["accessToken"];
            } else {
                return values["code"];
            }
        }

        static JToken GetJob(string token){
            var client = new RestClient("https://gene.lacuna.cc");
            var request = new RestRequest("/api/dna/jobs");
            string auth = "Bearer " + token;

            request.AddHeader("Authorization", auth);

            var response = client.Get(request);
            var content = response.Content; // Raw content as string
            var values = JObject.Parse(content)["job"];

            return values;
        }

        static string CriaUsuarioRestSharp(){
            var client = new RestClient("https://gene.lacuna.cc");

            var request = new RestRequest("/api/users/create");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    username = "ThiagoMT",
                    email = "thiago.tokarski2001@gmail.com",
                    password = "T5sDws1706"
                });

            var response = client.Post(request);
            var content = response.Content; // Raw content as string
            return content;
        }


        static void SendDecodeJob(string token, string job_id, string decoded_strand){
            var client = new RestClient("https://gene.lacuna.cc");

            string url = "/api/dna/jobs/" + job_id + "/decode";
            string auth = "Bearer " + token;

            var request = new RestRequest(url);
            request.AddHeader("Authorization", auth);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    strand = decoded_strand
                });

            var response = client.Post(request);
            var content = response.Content; // Raw content as string
            Console.WriteLine(content);
        }

        static void SendEncodeJob(string token, string job_id, string encoded_strand){
            var client = new RestClient("https://gene.lacuna.cc");

            string url = "/api/dna/jobs/" + job_id + "/encode";
            string auth = "Bearer " + token;

            var request = new RestRequest(url);
            request.AddHeader("Authorization", auth);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    strandEncoded = encoded_strand
                });

            var response = client.Post(request);
            var content = response.Content; // Raw content as string
            Console.WriteLine(content);
        }

        static void SendCheckGeneJob(string token, string job_id, bool isActivated){
            var client = new RestClient("https://gene.lacuna.cc");

            string url = "/api/dna/jobs/" + job_id + "/gene";
            string auth = "Bearer " + token;

            var request = new RestRequest(url);
            request.AddHeader("Authorization", auth);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    isActivated = isActivated
                });

            var response = client.Post(request);
            var content = response.Content; // Raw content as string
            Console.WriteLine(content);
        }


    }
}
