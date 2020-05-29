using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.BorannRemapping.Server.Data.AppContext;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace JPB.BubbleSearch
{
	public class Program
	{
		public SystemModel SolReference { get; set; }
		public float SearchDistance { get; set; }

		public SystemModel[] SystemsOfInterest { get; set; }

		public IList<long> VisitedSystems { get; set; }

		static async Task Main(string[] args)
		{
			var program = new Program();

			program.Init();
			await program.LoadData();
		}

		private async Task LoadData()
		{
			await LoadAndFilterSystems();
			//MatchBodies();
		}

		//private void MatchBodies()
		//{
		//	Console.WriteLine("Load Bodies");
		//	SystemBody[] bodies;
		//	using (var systemFs = new FileStream(@"C:\Users\Jean-\Desktop\bodies7days.json", FileMode.Open))
		//	{
		//		using (var systemStreamReader = new StreamReader(systemFs))
		//		{
		//			using (var jsonReader = new JsonTextReader(systemStreamReader))
		//			{
		//				var jsonSerializer = new JsonSerializer();
		//				bodies = jsonSerializer.Deserialize<SystemBody[]>(jsonReader);
		//			}
		//		}
		//	}

		//	Console.WriteLine("Match Bodies");
		//	var count = 0;
		//	Parallel.For(0, Systems.Length, (i, state) =>
		//	{
		//		var system = Systems[i];
		//		system.SystemBodies = bodies.Where(e => e.SystemId64 == system.Id64).ToArray();
		//		var e = Interlocked.Add(ref count, 1);
		//		if (e % 50 == 0)
		//		{
		//			Console.WriteLine($"{count} - {Systems.Length}");
		//		}
		//	});
		//	Console.WriteLine();

		//	var usefullSystems = SystemsOfInterest
		//		.Where(e => e.SystemBodies?.Any() == true)
		//		.Where(e => e.SystemBodies.Any(f => f.Rings?.Any(g => g.Type == "Icy") == true))
		//		.ToArray();

		//	var serializeObject = JsonConvert.SerializeObject(usefullSystems);
		//	File.WriteAllText("usefullSystems.json", serializeObject);
		//}

		private async Task LoadAndFilterSystems()
		{
			Console.WriteLine("Load Systems");
			var foundSystems = 0L;
			var skippedSystems = 0L;
			var seralizationQueue = new BlockingCollection<string>();
			var resultSet = new ConcurrentBag<SystemModel>();
			var threads = new Thread[Environment.ProcessorCount - 4];

			var processPerSecound = 0;
			var readingPerSecound = 0;
			for (int i = 0; i < threads.Length; i++)
			{
				var thread = new Thread(() =>
				{
					foreach (var toSerialize in seralizationQueue.GetConsumingEnumerable())
					{
						try
						{
							var deserializeObject = JsonConvert.DeserializeObject<SystemModel>(toSerialize);
							if (deserializeObject.InRangeOf(SolReference.Coordinates, SearchDistance))
							{
								resultSet.Add(deserializeObject);
								Interlocked.Add(ref foundSystems, 1);
							}
							else
							{
								Interlocked.Add(ref skippedSystems, 1);
							}
						}
						catch (Exception e)
						{
							Interlocked.Add(ref skippedSystems, 1);
							continue;
						}
						Interlocked.Add(ref processPerSecound, 1);
					}
				});
				thread.Name = "Eval" + i;
				threads[i] = thread;
				thread.Start();
			}


			var bytes = 0L;
			var size = 0L;
			var task = Task.Run(async () =>
			{
				var runningSince = DateTime.Now;
				while (!seralizationQueue.IsAddingCompleted || threads.Any(f => f.IsAlive))
				{
					await Task.Delay(1000);
					Console.CursorLeft = 0;
					var progress = Math.Round((((bytes / (decimal)size)) * 100), 2);
					Console.Write($"Progress: {progress}, " +
					              $"found: {foundSystems}, " +
					              $"skipped: {skippedSystems}, " + 
					              $"time: {(DateTime.Now - runningSince):g}, " +
					              $"Queued: {seralizationQueue.Count}," +
					              $"Reading/s: {readingPerSecound} " +
					              $"Writing/s: {processPerSecound} ");
					readingPerSecound = 0;
					processPerSecound = 0;
				}
			});

			await Task.Run(() =>
			{
				long counter = 0;
				using (var systemFs = new FileStream(@"H:\galaxy.json\galaxy.json",
					FileMode.Open, FileAccess.Read, FileShare.None, 2024, FileOptions.SequentialScan))
				{
					size = systemFs.Length;
					using (var streamReader = new StreamReader(systemFs, Encoding.UTF8, true))
					{
						while (!streamReader.EndOfStream)
						{
							if (seralizationQueue.Count > 1000000)
							{
								Thread.Sleep(TimeSpan.FromSeconds(15));
							}

							counter++;
							var line = streamReader.ReadLine().Trim(' ', ',', '\t', '\n', '\r');
							if (line.StartsWith('{') && line.EndsWith('}'))
							{
								seralizationQueue.Add(line);
							}

							readingPerSecound++;
							bytes = systemFs.Position;
						}
					}
				}
			});

			seralizationQueue.CompleteAdding();
			Console.WriteLine("Adding complete");
			foreach (var thread in threads)
			{
				thread.Join();
			}

			await task;

			Console.WriteLine("Serialize Objects");
			var serializeObject = JsonConvert.SerializeObject(resultSet.ToArray());
			File.WriteAllText("usefulSystems.json", serializeObject);

			Console.WriteLine("Done");
			Console.ReadLine();

			//SystemsOfInterest = JsonConvert.DeserializeObject<BorannRemapping.Server.Data.AppContext.System[]>(File.ReadAllText("usefullSystems.json"));

			//Console.WriteLine("Loaded " + SystemsOfInterest.Length);
			//SystemsOfInterest = SystemsOfInterest.Where(f => f.InRangeOf(SolReference.Coordinates, 600)).ToArray();
			//Console.WriteLine("Filtered " + SystemsOfInterest.Length);
		}

		public BorannRemapping.Server.Data.AppContext.System[] Systems { get; set; }

		private void Init()
		{
			SearchDistance = 200;
			VisitedSystems = new List<long>();
			SolReference = new SystemModel()
			{
				Name = "Sol",
				Coordinates = new CoordinatesModel()
				{
					Y = 0,
					Z = 0,
					X = 0
				}
			};

			//if (File.Exists("visitedSystems.json"))
			//{
			//	VisitedSystems = JsonConvert.DeserializeObject<List<long>>(File.ReadAllText("visitedSystems.json"));
			//}
		}
	}
}
