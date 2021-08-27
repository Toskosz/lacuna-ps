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
        private static RestClient client = new RestClient("https://gene.lacuna.cc");

        static void Main(string[] args)
        {
            /*var resposta = CriaUsuario();
            Console.WriteLine(resposta);*/

            for (int i = 0; i < 11; i++){

                token = GetToken();

                JToken job_values = GetJob();

                string job_id = job_values["id"].Value<string>();
                string job_type = job_values["type"].Value<string>();

                Console.WriteLine("The job is:");
                Console.WriteLine(job_type);

                switch (job_type)
                {
                    case "DecodeStrand":
                        string decoded_strand = DecodeStrandOpr(job_values["strandEncoded"].Value<string>());
                        SendDecodeJob(job_id, decoded_strand);
                        break;
                    case "EncodeStrand":
                        string encoded_strand = EncodeStrandOpr(job_values["strand"].Value<string>());
                        SendEncodeJob(job_id, encoded_strand);
                        break;
                    case "CheckGene":
                        bool isActivated = CheckGeneOpr(job_values["geneEncoded"].Value<string>(), job_values["strandEncoded"].Value<string>());
                        SendCheckGeneJob(job_id, isActivated);
                        break;
                }
            }
        }

        /* Base64 -> Hex -> Bin */
        static string BaseToBin(string strand){
            /*Base64 string conversion to ByteArray hexadecimal */
            byte[] decodedByteArray = Convert.FromBase64CharArray(strand.ToCharArray(), 0, strand.Length);
            
            /* Converts byteArray to string */
            string byte_string = BitConverter.ToString(decodedByteArray);
            byte_string = byte_string.Replace("-", "");

            /* Hex string to binary string */
            string binarystring = String.Join(String.Empty, byte_string.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

            return binarystring;
        }


        /* Decoding of strand */
        static string DecodeStrandOpr(string StrandEncoded){
            string decoded_strand = "";
            string bin_strand = BaseToBin(StrandEncoded);

            /* DNA decoding defined by README.md */
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

        /* Takes a string in hexadecimal and converts it into byte array (same content) */
        public static byte[] HexStringToHexByte(string inputHex)
        {
            /* prepares for long input */
            var resultantArray = new byte[inputHex.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++){
                resultantArray[i] = System.Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }

        /* Bin to Hex conversion function available at https://stackoverflow.com/questions/5612306/converting-long-string-of-binary-to-hex-c-sharp/17377983 */
        public static string BinaryStringToHexString(string binStrand)
        {
            if (string.IsNullOrEmpty(binStrand)){ 
                return binStrand;
            }

            StringBuilder result = new StringBuilder(binStrand.Length / 8 + 1);

            int mod4Len = binStrand.Length % 8;
            if (mod4Len != 0)
            {
                binStrand = binStrand.PadLeft(((binStrand.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binStrand.Length; i += 8)
            {
                string eightBits = binStrand.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        /* Encoding of a strand */
        static string EncodeStrandOpr(string strand) {
            string strandBin = "";

            /* encoding as presented in the README.md */
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

            /* conversions necessary to send the resulting strand */
            string strHex = BinaryStringToHexString(strandBin);
            byte[] byteHex = HexStringToHexByte(strHex);
            return System.Convert.ToBase64String(byteHex);
        }


        /* function that verifies if strand is complementary or not */
        static bool ComplementaryStrand(string strand){
            string start = strand.Substring(0, 3);
            if(start != "CAT"){
                return true;
            } else{
                return false;
            }

        }


        /* Complementary strand to template strand according to README.md */
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

        /* Check gene operation */
        static bool CheckGeneOpr(string geneEncoded, string strandEncoded){

            /* decodefication of the strands */
            string decoded_strand = DecodeStrandOpr(strandEncoded);
            string decoded_gene = DecodeStrandOpr(geneEncoded);

            /* verification of complementary strand */
            if (ComplementaryStrand(decoded_strand)){
                /* if complementary strand then translate it to template strand */
                decoded_strand = CompToTemplate(decoded_strand);
            }

            /* application of LCS algorithm */
            int lenght_of_substring = LCS(decoded_gene, decoded_strand, decoded_gene.Length, decoded_strand.Length);

            /* verification of percentage of the largest common substring(LCS) in the gene strand */
            if (((float)lenght_of_substring / (float)decoded_gene.Length) > 0.5){
                return true;
            }else{
                return false;
            }
        }

        /* function to get token from API */
        static string GetToken(){
            /* var client = new RestClient("https://gene.lacuna.cc"); */
            var request = new RestRequest("/api/users/login");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    username = "ThiagoMT",
                    password = "T5sDws1706"
                });

            var response = client.Post(request);
            var content = response.Content;

            /* String to json for easier acess */
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            

            if (values["code"].Equals("Success")){
                return values["accessToken"];
            } else {
                return values["code"];
            }
        }

        /* function to request new job */
        static JToken GetJob(){
            /* var client = new RestClient("https://gene.lacuna.cc"); */

            var request = new RestRequest("/api/dna/jobs");
            string auth = "Bearer " + token;

            request.AddHeader("Authorization", auth);

            var response = client.Get(request);
            var content = response.Content;

            /* json inside json parsing */
            var values = JObject.Parse(content)["job"];

            return values;
        }

        /* function to create user */
        static string CriaUsuario(){
            /* var client = new RestClient("https://gene.lacuna.cc"); */

            var request = new RestRequest("/api/users/create");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(
                new
                {
                    username = "ThiagoMT",
                    email = "thiago.tokarski2001@gmail.com",
                    password = "password" /* not my password */
                });

            var response = client.Post(request);
            var content = response.Content;
            return content;
        }

        /* -------- Functions to send jobs because of different request body structure -------- */



        static void SendDecodeJob(string job_id, string decoded_strand){
            /* var client = new RestClient("https://gene.lacuna.cc"); */

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
            var content = response.Content;

            /* feedback */
            Console.WriteLine(content);
        }

        static void SendEncodeJob(string job_id, string encoded_strand){
            /* var client = new RestClient("https://gene.lacuna.cc"); */

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
            var content = response.Content;

            /* feedback */
            Console.WriteLine(content);
        }

        static void SendCheckGeneJob(string job_id, bool isActivated){
            /* var client = new RestClient("https://gene.lacuna.cc"); */

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
            var content = response.Content;

            /* feedback */
            Console.WriteLine(content);
        }


    }
}
