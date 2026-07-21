using System;
using XuguClient;
var cs = "IP=192.168.2.239;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5287;AUTO_COMMIT=on;CHAR_SET=UTF8";
using var c = new XGConnection(cs);
c.Open();
using var cmd = c.CreateCommand();
cmd.CommandText = "SELECT TABLE_NAME FROM DBA_TABLES WHERE VALID='T' AND (IS_SYS='F' OR IS_SYS IS NULL) AND UPPER(TABLE_NAME) IN ('LANGUAGES','LEVEL1','MULTILINGUALSTRINGS','FIELDS','GLOBALIZATIONS','PRIMITIVECOLLECTIONSENTITY','CITIES','SQUADS','GEARS','WEAPONS','MISSIONS','TAGS','FACTIONS','LOCUSTLEADERS','LOCUSTHIGHCOMMANDS','SQUADMISSIONS','OFFICERS','LOCUSTCOMMANDERS','LOCUSTHORDES')";
using var r = cmd.ExecuteReader();
var tables = new System.Collections.Generic.List<string>();
while(r.Read()) tables.Add(r.GetString(0));
r.Close();
Console.WriteLine("count="+tables.Count);
foreach(var t in tables){ try{ using var d=c.CreateCommand(); d.CommandText=$"DROP TABLE `{t}` CASCADE"; d.ExecuteNonQuery(); Console.WriteLine("dropped "+t);} catch(Exception ex){ Console.WriteLine(ex.Message);} }
