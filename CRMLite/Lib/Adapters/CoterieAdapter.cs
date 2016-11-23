using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class CoterieAdapter : BaseAdapter<Attendance>
	{
		readonly Activity Context;
		readonly IList<Attendance> Attendancies;
		readonly List<Employee> Employees;

		public CoterieAdapter(Activity context, IList<Attendance> attendancies)
		{
			Context = context;
			Attendancies = attendancies;
			Employees = MainDatabase.GetItems<Employee>();
		}

		public override Attendance this[int position] {
			get { return Attendancies[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get { return Attendancies.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = Attendancies[position];
			// TODO: Fix
			var pharmacy = MainDatabase.GetEntity<Pharmacy>(item.Pharmacy);

			var coterieDatas = MainDatabase.GetDatas<CoterieData>(item.UUID);

			var brands = coterieDatas.GroupBy(cd => cd.Brand).Select(g => MainDatabase.GetItem<DrugBrand>(g.Key).name);
			// TODO: Fix
			var employees = new List<string>();
			foreach (var cd in coterieDatas.GroupBy(cd => cd.Employee)) {
				var emp = Employees.SingleOrDefault(e => e.UUID == cd.Key);
				if (emp != null) {
					employees.Add(emp.Name);
				}
			}

			var view = (convertView ??
								Context.LayoutInflater.Inflate(
				            	Resource.Layout.CoterieTableItem,
								parent,
								false)) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.ctiLegalNameTV).Text =
				    string.IsNullOrEmpty(pharmacy.LegalName) ? "<пусто>" : pharmacy.LegalName;

			view.FindViewById<TextView>(Resource.Id.ctiPharmacyTV).Text =
				    string.IsNullOrEmpty(pharmacy.GetName()) ? "<пусто>" : pharmacy.GetName();

			view.FindViewById<TextView>(Resource.Id.ctiWhenTV).Text = item.When.LocalDateTime.ToShortDateString();

			view.FindViewById<TextView>(Resource.Id.ctiDrugBrandsTV).Text = string.Join("; ", brands);
			view.FindViewById<TextView>(Resource.Id.ctiEmployeesTV).Text = string.Join("; ", employees);

			return view;
		}
	}
}
