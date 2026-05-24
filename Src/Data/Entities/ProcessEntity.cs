namespace SinaMN75U.Data.Entities;

// 1. تعریف فرآیند (مثل "احراز هویت")

[Table("Processes")]
public sealed class ProcessEntity : BaseEntity<TagProcess, BaseJson> {
	public required string Title { get; set; }
    
	public ICollection<ProcessStepEntity> Steps { get; set; } = [];
	public ICollection<UserProcessEntity> UserProcesses { get; set; } = [];
}

// 2. تعریف مرحله از فرآیند (مثل "آپلود کارت ملی")
[Table("ProcessSteps")]
public sealed class ProcessStepEntity : BaseEntity<TagProcessStep, BaseJson> {
	public required int StepNumber { get; set; } // مرحله 1، 2، 3
    
	public required string Instruction { get; set; } // توضیح برای کاربر: "لطفاً کارت ملی خود را آپلود کنید"
    
	public required Guid ProcessId { get; set; }
	public ProcessEntity Process { get; set; } = null!;
    
	public ICollection<UserDocumentEntity> UserDocuments { get; set; } = [];
}

// 3. رکورد شروع یک فرآیند توسط یک کاربر خاص
[Table("UserProcesses")]
public sealed class UserProcessEntity : BaseEntity<TagUserProcess, BaseJson> {
	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
    
	public required Guid ProcessId { get; set; }
	public ProcessEntity Process { get; set; } = null!;
    
	public required int CurrentStep { get; set; } // آخرین مرحله انجام شده
    
	public ICollection<UserDocumentEntity> Documents { get; set; } = [];
}

// 4. مدرک آپلود شده توسط کاربر برای یک مرحله مشخص
[Table("UserDocuments")]
public sealed class UserDocumentEntity : BaseEntity<TagUserDocument, BaseJson> {
	public required byte[] FileContent { get; set; }
	public required string FileName { get; set; }
	public string? RejectReason { get; set; } // دلیل رد شدن توسط ادمین
    
	public required Guid UserProcessId { get; set; }
	public UserProcessEntity UserProcess { get; set; } = null!;
    
	public required Guid ProcessStepId { get; set; }
	public ProcessStepEntity ProcessStep { get; set; } = null!;
}