using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;


public class Program
{
	public static void Main()
	{
		CriaUsuario("ThiagoMT","thiago.tokarski2001@gmail.com","T5sDws");
		Console.WriteLine("teste");
	}

	static void CriaUsuario(string usuario, string email, string senha){
		IEnumerable<KeyValuePair<string,string>> info = new List<KeyValuePair<string,string>>(){
			new KeyValuePair<string,string>("username",usuario),
			new KeyValuePair<string,string>("email",email),
			new KeyValuePair<string,string>("password",senha)
		};
		HttpContent conteudo = new FormUrlEncodedContent(info);
		string url = "https://gene.lacuna.cc/api/users/create";
		PostRequest(url, conteudo);
	}
	
	async static void GetRequest(string url){
		using(HttpClient client = new HttpClient()){
			using(HttpResponseMessage response = await client.GetAsync(url)){
				using(HttpContent content = response.Content){
					string resposta = await content.ReadAsStringAsync();
				}
			}
		}
	}
	
	async static void PostRequest(string url, HttpContent conteudo){
		using(HttpClient client = new HttpClient()){
			Console.WriteLine("aqui 1");
			using(HttpResponseMessage response = await client.PostAsync(url, conteudo)){
				Console.WriteLine("aqui 2");
				using(HttpContent content = response.Content){
					Console.WriteLine("aqui 3");
					string resposta = await content.ReadAsStringAsync();
					Console.WriteLine(resposta);
					Console.WriteLine("resposta acima");
				}
			}
		}
	}
}
