using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class EmployeeEditAdapter : BaseAdapter<Employee>
	{
		readonly Activity Context;
		readonly IList<Employee> Employees;
		readonly List<Position> Positions;

		public EmployeeEditAdapter(Activity context, IList<Employee> employees)
		{
			Context = context;
			Employees = employees;
			Positions = MainDatabase.GetItems<Position>();

		}

		public override Employee this[int position] {
			get { return Employees[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get { return Employees.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = Employees[position];

			var view = (convertView ??
								Context.LayoutInflater.Inflate(
								Resource.Layout.EmployeeEditTableItem,
								parent,
								false)) as LinearLayout;

			view.SetTag(Resource.String.EmployeeUUID, item.UUID);
			view.SetTag(Resource.String.IsChanged, false);

			var name = view.FindViewById<EditText>(Resource.Id.eetiNameET);
			name.Text = string.IsNullOrEmpty(item.Name) ? string.Empty : item.Name;
			name.AfterTextChanged -= ET_AfterTextChanged;
			name.AfterTextChanged += ET_AfterTextChanged;


			/* <Position> */
			var pos = view.FindViewById<Spinner>(Resource.Id.eetiPositionS);
			var positionAdapter = new ArrayAdapter(
				Context, Android.Resource.Layout.SimpleSpinnerItem, Positions.Select(x => x.name).ToArray()
			);
			positionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			pos.Adapter = positionAdapter;
			pos.ItemSelected -= Pos_ItemSelected;
			pos.ItemSelected += Pos_ItemSelected;
			/* </Position> */
			if (!string.IsNullOrEmpty(item.Position)) {
				pos.SetSelection(Positions.FindIndex(e => string.Compare(e.uuid, item.Position) == 0));
			}

			var isCustomer = view.FindViewById<CheckBox>(Resource.Id.eetiIsCustomerCB);
			isCustomer.Checked = item.IsCustomer;
			isCustomer.CheckedChange -= CB_CheckedChange;
			isCustomer.CheckedChange += CB_CheckedChange;

			var birthDate = view.FindViewById<EditText>(Resource.Id.eetiBirthDateET);
			birthDate.Text = item.BirthDate.HasValue ? item.BirthDate.Value.ToString("dd.MM.yyyy") : string.Empty;
			birthDate.AfterTextChanged -= ET_AfterTextChanged;
			birthDate.AfterTextChanged += ET_AfterTextChanged;

			var phone = view.FindViewById<EditText>(Resource.Id.eetiPhoneET);
			phone.Text = string.IsNullOrEmpty(item.Phone) ? string.Empty : item.Phone;
			phone.AfterTextChanged -= ET_AfterTextChanged;
			phone.AfterTextChanged += ET_AfterTextChanged;

			var email = view.FindViewById<EditText>(Resource.Id.eetiEmailET);
			email.Text = string.IsNullOrEmpty(item.Email) ? string.Empty : item.Email;
			email.AfterTextChanged -= ET_AfterTextChanged;
			email.AfterTextChanged += ET_AfterTextChanged;

			var canParticipate = view.FindViewById<CheckBox>(Resource.Id.eetiCanParticipateCB);
			canParticipate.Checked = item.CanParticipate;
			canParticipate.CheckedChange -= CB_CheckedChange;
			canParticipate.CheckedChange += CB_CheckedChange;

			var comment = view.FindViewById<EditText>(Resource.Id.eetiCommentET);
			comment.Text = string.IsNullOrEmpty(item.Comment) ? string.Empty : item.Comment;
			comment.AfterTextChanged -= ET_AfterTextChanged;
			comment.AfterTextChanged += ET_AfterTextChanged;

			//var del = view.FindViewById<RelativeLayout>(Resource.Id.eetiDeleteRL);
			//del.Click -= Del_Click;
			//del.Click += Del_Click;

			return view;
		}

		void Pos_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			if (sender is Spinner) {
				var spinner = ((Spinner)sender);
				var row = (LinearLayout)spinner.Parent;
				row.SetTag(Resource.String.IsChanged, true);
				spinner.SetTag(Resource.String.PositionUUID, Positions[spinner.SelectedItemPosition].uuid);
			}
		}

		void ET_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			if (sender is EditText) {
				var row = (LinearLayout)((EditText)sender).Parent;
				row.SetTag(Resource.String.IsChanged, true);
			}
		}

		void CB_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (sender is CheckBox) {
				var row = (LinearLayout)((CheckBox)sender).Parent.Parent;
				row.SetTag(Resource.String.IsChanged, true);
			}
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
