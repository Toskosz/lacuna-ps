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

	async static void CriaUsuario(string usuario, string email, string senha){
		var values = new Dictionary<string, string>{
			{ "username", usuario },
			{ "email", email },
			{ "password", senha}
		};
		
		var data = new FormUrlEncodedContent(values);
		var url = "https://gene.lacuna.cc/api/users/create";
		
		using(var client = new HttpClient()){
			var response = await client.PostAsync(url, data);
			string result = response.Content.ReadAsStringAsync().Result;
			Console.WriteLine(result);
			Console.WriteLine("1");
		}
	}
}
