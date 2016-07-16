using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class EmployeeAdapter : BaseAdapter<Employee>
	{
		Activity context = null;
		readonly IList<Employee> employees = new List<Employee>();

		public EmployeeAdapter(Activity context, IList<Employee> employees) : base()
		{
			this.context = context;
			this.employees = employees;
		}

		public override Employee this[int position]
		{
			get { return employees[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return employees.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = employees[position];

			var view = (convertView ??
								context.LayoutInflater.Inflate(
				            	Resource.Layout.EmployeeTableItem,
								parent,
								false)) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.etiNameTV).Text = string.IsNullOrEmpty(item.Name) ? "<пусто>" : item.Name;
			view.FindViewById<TextView>(Resource.Id.etiPositionTV).Text = string.IsNullOrEmpty(item.Position) ? "<пусто>" : item.Position;
			view.FindViewById<TextView>(Resource.Id.etiBirthDateTV).Text = item.BirthDate == null ? "<пусто>" : item.BirthDate.Value.ToString("dd.MM.yyyy");
			//view.FindViewById<TextView>(Resource.Id.etiLoyaltyTV).Text = string.IsNullOrEmpty(item.Loyalty) ? "<пусто>" : item.Loyalty;
			view.FindViewById<CheckBox>(Resource.Id.etiIsCustomerCB).Checked = item.IsCustomer;

			return view;
		}
	}
}
