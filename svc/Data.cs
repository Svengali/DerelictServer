using Microsoft.AspNetCore.Http.Json;

using System.Text;
using System.Text.Json;



namespace svc;

static public class Data
{

	static public void Save<T>( T data ) => Data<T>.Save( data );
	static public void Save<T>( T data, string key ) => Data<T>.Save( data, key );

	static public void Load<T>( string key, out T? data ) => data = Data<T>.Load( key );

}


static public class Data<T>
{
	// TODO CONFIG Build a config from this
	static public string BaseDir = "bad";
	static public string BackupBaseDir = "backup_bad";
	static public Func<T, string>? FnKey = (d) => d.ToString();

	static public string GetFilename( string key )
	{
		return $"{BaseDir}/{key}.json";
	}

	static public string GetSaveFilename( string key )
	{
		var date = DateTime.UtcNow.ToString("yyyyMMDD_HHmmss");
		return $"{BackupBaseDir}/{key}.json.{date}.json";
	}


	static public void Save( T data )
	{
		Save( data, FnKey( data ) );
	}

	static public void Save( T data, string key )
	{
		JsonOptions options = new JsonOptions();
		options.SerializerOptions.WriteIndented = true;

		string file = GetFilename( key );

		if( File.Exists( file ) )
		{
			log.info( $"Save file {file} exists, backing up." );
			var saveFilename = GetSaveFilename( key );

			File.Move( file, saveFilename );
		}

		using FileStream createStream = File.Create( file );

		var jsonStr = JsonSerializer.Serialize( value: data );

		createStream.WriteAsync( Encoding.UTF8.GetBytes( jsonStr ) );

		//JsonSerializer.SerializeAsync( createStream, data );

		createStream.DisposeAsync();
	}

	static public T? Load( string key )
	{
		string file = GetFilename( key );

		var stream = File.OpenRead( file );

		string jsonString = File.ReadAllText( file );

		T? data = JsonSerializer.Deserialize<T>(jsonString);

		return data;
	}

	static public void Load( string key, out T? data )
	{
		data = Load( key );
	}


}
