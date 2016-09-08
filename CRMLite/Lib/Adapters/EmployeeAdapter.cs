using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class EmployeeAdapter : BaseAdapter<Employee>
	{
		readonly Activity Context;
		readonly IList<Employee> Employees;

		public EmployeeAdapter(Activity context, IList<Employee> employees)
		{
			Context = context;
			Employees = employees;
		}

		public override Employee this[int position]
		{
			get { return Employees[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return Employees.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = Employees[position];

			var view = (convertView ??
								Context.LayoutInflater.Inflate(
								Resource.Layout.EmployeeTableItem,
								parent,
								false)) as LinearLayout;

			view.SetTag(Resource.String.EmployeeUUID, item.UUID);

			view.FindViewById<TextView>(Resource.Id.etiNameTV).Text =
				    string.IsNullOrEmpty(item.Name) ? "<пусто>" : item.Name;

			view.FindViewById<TextView>(Resource.Id.etiPositionTV).Text =
				    string.IsNullOrEmpty(item.Position) ? "<пусто>" : MainDatabase.GetItem<Position>(item.Position).name;

			view.FindViewById<CheckBox>(Resource.Id.etiIsCustomerCB).Checked = item.IsCustomer;

			view.FindViewById<TextView>(Resource.Id.etiBirthDateTV).Text =
				    item.BirthDate.HasValue ? item.BirthDate.Value.ToString("dd.MM.yyyy") : "<пусто>";
			
			view.FindViewById<TextView>(Resource.Id.etiPhoneTV).Text =
				    string.IsNullOrEmpty(item.Phone) ? "<пусто>" : item.Phone;
			
			view.FindViewById<TextView>(Resource.Id.etiEmailTV).Text =
				    string.IsNullOrEmpty(item.Email) ? "<пусто>" : item.Email;
			
			view.FindViewById<CheckBox>(Resource.Id.etiCanParticipateCB).Checked = item.CanParticipate;

			view.FindViewById<TextView>(Resource.Id.etiCommentTV).Text =
				    string.IsNullOrEmpty(item.Comment) ? "<пусто>" : item.Comment;


			var del = view.FindViewById<RelativeLayout>(Resource.Id.etiDeleteRL);
			del.Click -= Del_Click;
			del.Click += Del_Click;

			return view;
		}

		void Del_Click(object sender, System.EventArgs e)
		{
			if (sender is RelativeLayout) {
				var row = ((RelativeLayout)sender).Parent as LinearLayout;
				var employeeUUID = (string)row.GetTag(Resource.String.EmployeeUUID);
				if (string.IsNullOrEmpty(employeeUUID)) return;

				foreach (var item in Employees) {
					if (item.UUID == employeeUUID) {
						Employees.Remove(item);
						break;
					}
				}

				MainDatabase.DeleteEntity<Employee>(employeeUUID);

				NotifyDataSetChanged();
			}
		}
	}
}
