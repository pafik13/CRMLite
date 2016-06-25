using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Text;

namespace CRMLite
{
	public class FixedSizedQueue<T>
	{
		readonly ConcurrentQueue<T> Q = new ConcurrentQueue<T>();

		public string Path { get; }
		public int Limit { get;  }

		public FixedSizedQueue (string path, int limit)
		{
			Path = path;
			Limit = limit;
		}


		public void Enqueue(T obj)
		{
			Q.Enqueue(obj);
			lock (this)
			{
				if (Q.Count < Limit)
				{
					T overflow;
					var builder = new StringBuilder();

					while (Q.Count > Limit && Q.TryDequeue(out overflow))
					{
						builder.AppendLine(JsonConvert.SerializeObject(overflow));
					}

					System.IO.File.AppendAllText(Path, builder.ToString());
				}
			}
		}
	}
}

