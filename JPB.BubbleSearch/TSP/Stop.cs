//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace JPB.BubbleSearch.TSP
//{
//	public static class Helper
//	{
//		//take an ordered list of nodes and set their next properties
//		public static void Connect(this IEnumerable<Stop> stops, bool loop)
//		{
//			Stop prev = null, first = null;
//			foreach (var stop in stops)
//			{
//				if (first == null)
//				{
//					first = stop;
//				}

//				if (prev != null)
//				{
//					prev.Next = stop;
//				}

//				prev = stop;
//			}

//			if (loop)
//			{
//				prev.Next = first;
//			}
//		}


//		//T with the smallest func(T)
//		public  static T MinBy<T, TComparable>(
//			this IEnumerable<T> xs,
//			Func<T, TComparable> func)
//			where TComparable : IComparable<TComparable>
//		{
//			return xs.DefaultIfEmpty().Aggregate(
//				(maxSoFar, elem) =>
//					func(elem).CompareTo(func(maxSoFar)) > 0 ? maxSoFar : elem);
//		}


//		//return an ordered nearest neighbor set
//		public  static IEnumerable<Stop> NearestNeighbors(this IEnumerable<Stop> stops)
//		{
//			//stop.System.Distance(stop.Next.System.Coordinates)
//			var stopsLeft = stops.ToList();
//			for (var stop = stopsLeft.First();
//				stop != null;
//				stop = stopsLeft.MinBy(s => stop.System.Distance(s.System.Coordinates)))
//			{
//				stopsLeft.Remove(stop);
//				yield return stop;
//			}
//		}
//	}

//	public class Stop
//	{
//		public System System { get; }

//		public Stop(System system)
//		{
//			System = system;
//		}

//		public Stop Next { get; set; }

//		public Stop Clone()
//		{
//			return new Stop(System);
//		}

//		public IEnumerable<Stop> CanGetTo()
//		{
//			var current = this;
//			while (true)
//			{
//				yield return current;
//				current = current.Next;
//				if (current == this)
//				{
//					break;
//				}
//			}
//		}

//		public override bool Equals(object obj)
//		{
//			return System == ((Stop)obj).System;
//		}
		
//		public override int GetHashCode()
//		{
//			return System.GetHashCode();
//		}
//	}

//	public class Tour
//	{
//		public Tour(IEnumerable<Stop> stops)
//		{
//			Anchor = stops.First();
//		}

//		public Stop Anchor { get; set; }

//		public IEnumerable<Tour> GenerateMutations()
//		{
//			for (Stop stop = Anchor; stop.Next != Anchor; stop = stop.Next)
//			{
//				//skip the next one, since you can't swap with that
//				Stop current = stop.Next.Next;
//				while (current != Anchor)
//				{
//					yield return CloneWithSwap(stop.System, current.System);
//					current = current.Next;
//				}
//			}
//		}

//		public Tour CloneWithSwap(System firstCity, System secondCity)
//		{
//			Stop firstFrom = null, secondFrom = null;
//			var stops = UnconnectedClones();
//			stops.Connect(true);

//			foreach (Stop stop in stops)
//			{
//				if (stop.System == firstCity)
//				{
//					firstFrom = stop;
//				}

//				if (stop.System == secondCity)
//				{
//					secondFrom = stop;
//				}
//			}

//			//the swap part
//			var firstTo = firstFrom.Next;
//			var secondTo = secondFrom.Next;

//			//reverse all of the links between the swaps
//			firstTo.CanGetTo()
//				.TakeWhile(stop => stop != secondTo)
//				.Reverse()
//				.Connect(false);

//			firstTo.Next = secondTo;
//			firstFrom.Next = secondFrom;

//			var tour = new Tour(stops);
//			return tour;
//		}


//		public IList<Stop> UnconnectedClones()
//		{
//			return Cycle().Select(stop => stop.Clone()).ToList();
//		}

//		public double Cost()
//		{
//			return Cycle().Aggregate(
//				0.0,
//				(sum, stop) =>
//					sum + stop.System.Distance(stop.Next.System.Coordinates));
//		}

//		private IEnumerable<Stop> Cycle()
//		{
//			return Anchor.CanGetTo();
//		}
//	}
//}
