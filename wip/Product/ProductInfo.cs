namespace Huybrechts.Core.Architect;

public record ArchitectElement: Entity, IEntity
{
    // Part of bigger element?
    public Ulid ParentId { get; set; }

    // Belongs to a specific suite or product group or suite
    public Ulid GroupId { get; set; }

    // Current/default vendor
    public Ulid VendorId { get; set; }

    // Current contract
    public Ulid VendorContractId { get; set; }

    // Full name of the product
    public string Name { get; set; } = string.Empty;

    // Short name of the product
    public string ShortName { get; set; } = string.Empty;

    // Product version
    public string Version { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string StateReason { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Subcategory { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string SupportState { get; set; } = string.Empty;

    public string SupportReason { get; set; } = string.Empty;

    public string DeploymentModel { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }

    public DateTime EndOfLifeDate { get; set; }

    public string EndOfLifePlan { get; set; } = string.Empty;

    public string ComplianceStatus { get; set; } = string.Empty;
    public string RiskAssessment { get; set; } = string.Empty;
    public string MaintenanceSchedule { get; set; } = string.Empty;
    public string PerformanceMetrics { get; set; } = string.Empty;
    public string BusinessAlignment { get; set; } = string.Empty;
    public string ValueProposition { get; set; } = string.Empty;
    public string UserSatisfaction { get; set; } = string.Empty;
    public string TrainingAndSupportResources { get; set; } = string.Empty;
    public string IntegrationPoints { get; set; } = string.Empty;
    public string BackupAndRecoveryPlan { get; set; } = string.Empty;
    public string ConfigurationManagement { get; set; } = string.Empty;
    public string ChangeHistory { get; set; } = string.Empty;
    public string TotalCostOfOwnership { get; set; } = string.Empty;
    public string BudgetAllocation { get; set; } = string.Empty;
    public string ResourceUtilization { get; set; } = string.Empty;
    
    public string ScalabilityOptions { get; set; } = string.Empty;
    public string SecurityMeasures { get; set; } = string.Empty;
}

public record ArchitectLifecycle: Entity, IEntity
{
    public int Sequence { get; set;}

    public string Name { get; set;} = string.Empty;

    public string State { get; set;} = string.Empty;

    public string Stage { get; set;} = string.Empty;

    public string Version { get; set;} = string.Empty;

    public string ReleaseInfo { get; set;} = string.Empty;

    public DateTime ProposedOn { get; set;}

    public string ProposedBy { get; set;} = string.Empty;

    public DateTime ApprovedOn { get; set;}

    public string ApprovedBy { get; set;} = string.Empty;
}





















/*

public record ArchitectStereotype: Entity, IEntity
{
    public string Name { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class EaScript: Entity, IEntity
{
    public Ulid EaArchetypeId { get; set; }

    public Ulid EaStereotypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Script { get; set; } = string.Empty;
}

public class EaPackage: Entity, IEntity
{
    public Ulid ParentId { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class EaElement2: Entity, IEntity
{
    public Ulid EaPackageId { get; set; }

    public Ulid EaStereotypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}

public class EaRelationship: Entity, IEntity
{
    public Ulid SourceId { get; set; }

    public Ulid TargetId { get; set; }

    public Ulid EaStereotype { get; set; }
}

public class EaDecision: Entity, IEntity
{
    public Ulid EaElementId { get; set; }

    public string State { get; set; } = string.Empty;

    public DateTime DecidedOn { get; set; }

    public string DecidedBy { get; set; } = string.Empty;
}

public class EaRequirements: Entity, IEntity
{


}

public class EaRequirements: Entity, IEntity
{


}

public class EaRequirements: Entity, IEntity
{


}

public class EaRequirements: Entity, IEntity
{


}

public class EaRequirements: Entity, IEntity
{


}
*/