using System;
using Realms;

namespace CRMLite.Entities
{
	public interface IAttendanceControl
	{
		void OnAttendanceStart(Attendance current);

		void OnAttendanceStop(Transaction openedTransaction, Attendance current);

		void OnAttendanceResume();

		void OnAttendancePause();
	}
}

