using System;
using Realms;

namespace CRMLite.Entities
{
	public interface IAttendanceControl
	{
		void OnAttendanceStart(DateTimeOffset? start);

		void OnAttendanceStop(Transaction openedTransaction, Attendance current);
	}
}

