using System;

namespace Referral2.Models.ViewModels.Forms
{
    public partial class PregnantViewModel
    {
        public PregnantViewModel(PregnantForm form, Baby baby)
        {
            Code = form.Code;
            ReferringMd = GlobalFunctions.GetMDFullName(form.ReferredByNavigation);
            RecordNumber = form.RecordNo;
            DateReferred = form.ReferredDate.ToString("dd/MM/yyyy");
            ReferringMdContact = form.ReferredByNavigation.Contact;
            Facility = form.ReferringFacilityNavigation == null ? "" : form.ReferringFacilityNavigation.Name;
            FacilityContact = form.ReferringFacilityNavigation == null ? "" : form.ReferringFacilityNavigation.Contact;
            HealthWorker = form.HealthWorker;
            ReferredTo = form.ReferredToNavigation == null ? "" : form.ReferredToNavigation.Name;
            Department = form.Department == null ? "" : form.Department.Description;
            ReferredToAddress = GlobalFunctions.GetAddress(form.ReferredToNavigation);
            WomanName = GlobalFunctions.GetFullName(form.PatientWoman);
            WomanAge = GlobalFunctions.ComputeAge(form.PatientWoman.DateOfBirth);
            WomanAddress = GlobalFunctions.GetAddress(form.PatientWoman);
            WomanReason = form.WomanReason;
            WomanFindings = form.WomanMajorFindings;
            WomanBeforeTreatment = form.WomanBeforeTreatment;
            WomanBeforeGivenTime = GlobalFunctions.GetDate((DateTime)form.WomanBeforeGivenTime, "dd/MM/yyyy");
            WomanDuringTransport = form.WomanDuringTransport;
            WomanDuringGivenTime = GlobalFunctions.GetDate((DateTime)form.WomanTransportGivenTime, "dd/MM/yyyy");
            WomanInformationGiven = form.WomanInformationGiven;
            BabyName = form.PatientBaby == null? "" : GlobalFunctions.GetFullName(form.PatientBaby);
            BabyDob = form.PatientBaby == null ? "" : form.PatientBaby.DateOfBirth.ToString("dd/MM/yyyy");
            BabyWeight = baby == null ? "" : baby.Weight == 0 ? "" : baby.Weight.ToString();
            BabyGestationAge = baby == null? "" : baby.GestationalAge.ToString();
            BabyReason = form.BabyReason;
            BabyFindings = form.BabyMajorFindings;
            BabyLastFeed = GlobalFunctions.GetDate((DateTime)form.BabyLastFeed, "dd/MM/yyyy");
            BabyBeforeTreatment = form.BabyBeforeTreatment;
            BabyBeforeGivenTime = GlobalFunctions.GetDate((DateTime)form.BabyBeforeGivenTime, "dd/MM/yyyy");
            BabyDuringTransport = form.BabyDuringTransport;
            BabyDuringGivenTime = GlobalFunctions.GetDate((DateTime)form.BabyTransportGivenTime, "dd/MM/yyyy");
            BabyInformationGiven = form.BabyInformationGiven;
        }
        public string Code { get; set; }
        public string ReferringMd { get; set; }
        public string RecordNumber { get; set; }
        public string DateReferred { get; set; }
        public string DateArrived { get; set; }
        public string ReferringMdContact { get; set; }
        public string Facility { get; set; }
        public string FacilityContact { get; set; }
        public string HealthWorker { get; set; }
        public string ReferredTo { get; set; }
        public string Department { get; set; }
        public string ReferredToAddress { get; set; }
        public string WomanName { get; set; }
        public int WomanAge { get; set; }
        public string WomanAddress { get; set; }
        public string WomanReason { get; set; }
        public string WomanFindings { get; set; }
        public string WomanBeforeTreatment { get; set; }
        public string WomanBeforeGivenTime { get; set; }
        public string WomanDuringTransport { get; set; }
        public string WomanDuringGivenTime { get; set; }
        public string WomanInformationGiven { get; set; }
        public string BabyName { get; set; }
        public string BabyDob { get; set; }
        public string BabyWeight { get; set; }
        public string BabyGestationAge { get; set; }
        public string BabyReason { get; set; }
        public string BabyFindings { get; set; }
        public string BabyLastFeed { get; set; }
        public string BabyBeforeTreatment { get; set; }
        public string BabyBeforeGivenTime { get; set; }
        public string BabyDuringTransport { get; set; }
        public string BabyDuringGivenTime { get; set; }
        public string BabyInformationGiven { get; set; }
    }
}
