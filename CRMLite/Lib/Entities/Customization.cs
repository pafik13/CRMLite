using Realms;

namespace CRMLite.Entities
{
	public class Customization : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }
		
		public string key { get; set; }
		
		public string type { get; set; }

		public string value { get; set; }
	}
	
	public static class Customizations
	{
		public const string AttendanceMinPeriod = "AttendanceMinPeriod";
		public const string IsPharmacyAddEnable = "IsPharmacyAddEnable";

		#region Locator customizations
		public const string IsLocatorEnable = "IsLocatorEnable";

		public const string IsLocatorNetRequestOn = "IsLocatorNetRequestOn";
		public const string LocatorNetRequestPeriod = "LocatorNetRequestPeriod";

		public const string IsLocatorGPSRequestOn = "IsLocatorGPSRequestOn";
		public const string LocatorGPSRequestPeriod = "LocatorGPSRequestPeriod";

		public const string LocatorIdlePeriod = "LocatorIdlePeriod";
		#endregion

		#region PhotoUploader customizations
		public const string IsPhotoUploaderEnable = "IsPhotoUploaderEnable";
		#endregion
		
		
		#region Reserved UUIDs customizations
		public const string InternshipUUID = "InternshipUUID";
		public const string SickleaveUUID = "SickleaveUUID";
		public const string TrainingFullUUID = "TrainingFullUUID";
		public const string TrainingHalfUUID = "TrainingHalfUUID";
		public const string WorkleaveUUID = "WorkleaveUUID";
		#endregion
		
		/*
		TRAININGHALF : 54524149-4e49-4000-0000-4e4748414c46
		TRAININGFULL : 54524149-4e49-4000-0000-4e4746554c4c
		INTERNSHIP00 : 494e5445-524e-4000-0000-534849503030
		SICKLEAVE000 : 5349434b-4c45-4000-0000-415645303030
		WORKLEAVE000 : 574f524b-4c45-4000-0000-415645303030


		$.post(
		  "Customization", JSON.stringify(cust), function() { alert( "success")},"json"
		).done(function() {
			alert( "second success" );
		  })
		  .fail(function() {
			alert( "error" );
		  })
		  .always(function() {
			alert( "finished" );
		  });
		  
		var cust = {
			name: "UUID для отражения в маршруте тренировки на половину дня",
			key: 'HalfDayTrainingUUID',
			type: 'string',
			value : '54524149-4e49-4000-0000-4e4748414c46'
		}

		CREATE OR REPLACE FUNCTION public.f_const_ri_workleave()
		  RETURNS uuid
		  LANGUAGE sql
		  IMMUTABLE
		AS $function$SELECT uuid '574f524b-4c45-4000-0000-415645303030'::uuid $function$


		ALTER TABLE pharmacy ADD CONSTRAINT uuidchk CHECK (uuid NOT IN ('54524149-4e49-4000-0000-4e4748414c46', '54524149-4e49-4000-0000-4e4746554c4c', '494e5445-524e-4000-0000-534849503030', '5349434b-4c45-4000-0000-415645303030', '574f524b-4c45-4000-0000-415645303030'));
		
		*/

	}
}
