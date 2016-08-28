using System;
using Realms;

namespace CRMLite
{
	public interface IAttendanceControl
	{
		void OnAttendanceStart(DateTimeOffset? start);

		void OnAttendanceStop(Transaction openedTransaction, Attendance current);
	}
}

