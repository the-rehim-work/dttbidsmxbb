using dttbidsmxbb.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace dttbidsmxbb.Models
{
    public class Information
    {
        public int Id { get; set; }

        [Display(Name = "Hərbi hissə")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public int MilitaryBaseId { get; set; }
        [Display(Name = "Hərbi hissə")]
        public virtual MilitaryBase? MilitaryBase { get; set; }

        [Display(Name = "Göndərən Hərbi hissə")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public int SenderMilitaryBaseId { get; set; }
        [Display(Name = "Göndərən Hərbi hissə")]
        public virtual MilitaryBase? SenderMilitaryBase { get; set; }

        [Display(Name = "Göndərilmə nömrəsi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string SentSerialNumber { get; set; } = string.Empty;

        [Display(Name = "Göndərilmə tarixi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public DateOnly SentDate { get; set; }

        [Display(Name = "DTTBİ-ə daxil olma nömrəsi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string ReceivedSerialNumber { get; set; } = string.Empty;

        [Display(Name = "DTTBİ-ə daxil olma tarixi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public DateOnly ReceivedDate { get; set; }

        [Display(Name = "Buraxılışı rəsmiləşdirilən şəxsin rütbəsi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public int MilitaryRankId { get; set; }
        [Display(Name = "Buraxılışı rəsmiləşdirilən şəxsin rütbəsi")]
        public virtual MilitaryRank? MilitaryRank { get; set; }

        [Display(Name = "Buraxılışın rəsmiləşdirildiyi vəzifə")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string RegardingPosition { get; set; } = string.Empty;

        [Display(Name = "Buraxılışı rəsmiləşdirilən şəxsin vəzifəsi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Buraxılışı rəsmiləşdirilən şəxsin Soyadı")]
        public string? Lastname { get; set; }

        [Display(Name = "Buraxılışı rəsmiləşdirilən şəxsin Adı")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string Firstname { get; set; } = string.Empty;

        [Display(Name = "Buraxılışı rəsmiləşdirilən şəxsin Atasının adı")]
        public string? Fathername { get; set; }

        [Display(Name = "Buraxılışlı vəzifəyə təyin olunma tarixi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public DateOnly AssignmentDate { get; set; }

        [Display(Name = "Tələb olunan buraxılışın forması")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public PrivacyLevel PrivacyLevel { get; set; }

        [Display(Name = "Buraxılışın DTX-a göndərilmə nömrəsi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string? SendAwaySerialNumber { get; set; } = string.Empty;

        [Display(Name = "Buraxılışın DTX-a göndərilmə tarixi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public DateOnly? SendAwayDate { get; set; }

        [Display(Name = "İcraçı")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public int ExecutorId { get; set; }
        [Display(Name = "İcraçı")]
        public virtual Executor? Executor { get; set; }

        [Display(Name = "Rəsmiləşdirilmiş buraxılış vərəqəsinin nömrəsi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public string? FormalizationSerialNumber { get; set; } = string.Empty;

        [Display(Name = "Rəsmiləşdirilmiş buraxılış vərəqəsinin tarixi")]
        [Required(ErrorMessage = "{0} sahəsi mütləqdir.")]
        public DateOnly? FormalizationDate { get; set; }

        [Display(Name = "DTX tərəfindən şəxsin buraxılış sənədlərinə imtina bildirilməsi barədə məlumat")]
        public string? RejectionInfo { get; set; }

        [Display(Name = "Şəxslərin buraxılış sənədlərini rəsmiləşdirilmədən h/h-ə geri qaytarılması barədə məlumat")]
        public string? SentBackInfo { get; set; }

        [Display(Name = "Qeyd")]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}
