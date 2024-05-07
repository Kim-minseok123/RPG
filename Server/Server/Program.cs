using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
            string relativePath = "../../../../../MMO_Maple/Builds/Win64/DedicatedServer/DedicatedServer.exe";
            string fullPath = Path.GetFullPath(relativePath, Environment.CurrentDirectory);
            Thread.Sleep(5000);
            StartExternalProgram(fullPath);
            while (true)
			{
				GameLogic.Instance.Update();
				Thread.Sleep(0);
            }
        }
		static void DbTask()
		{
            while (true)
            {
                DbTransaction.Instance.Flush();
				Thread.Sleep(0);
            }
        }
		static void NetworkTask()
		{
            while (true)
            {
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach (var session in sessions)
                {
					session.FlushSend();
                }
                Thread.Sleep(0);
            }
        }
		static void StartServerInfoTask()
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
				{
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if(serverDb != null)
					{
						serverDb.IpAddress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
					}
					else
					{
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							Port = Program.Port,
							IpAddress = Program.IpAddress,
                            BusyScore = SessionManager.Instance.GetBusyScore(),
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
					}

				}
			});
			t.Interval = 10 * 1000;
			t.Start();
		}
		public static string Name { get; } = "테스피아";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }
		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Add(1);
            });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			StartServerInfoTask();

            // GameLogicTask
            {
				Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
			}

            {
                Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }
			Thread.CurrentThread.Name = "GameLogic";
			GameLogicTask();
        }
        static void StartExternalProgram(string path)
        {
            try
            {
				Console.WriteLine("Resolved path to executable: " + Path.GetFullPath(path));

                ProcessStartInfo startInfo = new ProcessStartInfo(path)
                {
                    Arguments = "-batchmode -nographics",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process process = new Process { StartInfo = startInfo };
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start external program: " + ex.Message);
            }
        }
    }
}
