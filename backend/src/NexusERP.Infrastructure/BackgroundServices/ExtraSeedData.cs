using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Infrastructure.BackgroundServices;

internal static class ExtraSeedData
{
    internal static Project[] Projects =>
    [
        P("Accounts Payable Automation", "PRJ-FIN-015", "Automate invoice capture, approval routing, and payment scheduling.", ProjectStatus.Active, 88000, 2026, 3, 1, 2026, 10, 31),
        P("Inventory Forecasting Engine", "PRJ-INV-016", "ML-based demand forecasting and reorder point optimization.", ProjectStatus.Active, 112000, 2026, 2, 15, 2026, 11, 30),
        P("Payroll Modernization", "PRJ-PAY-017", "Cloud payroll with multi-country tax rules and direct deposit.", ProjectStatus.Planning, 76000, 2026, 6, 1, 2027, 3, 31),
        P("Expense Management System", "PRJ-EXP-018", "Mobile receipt capture, policy enforcement, and reimbursement workflows.", ProjectStatus.Active, 54000, 2026, 1, 10, 2026, 8, 31),
        P("Asset Tracking Platform", "PRJ-ASG-019", "RFID and barcode tracking for IT and office assets.", ProjectStatus.Active, 67000, 2026, 4, 1, 2026, 12, 15),
        P("Contract Lifecycle Management", "PRJ-CON-020", "Contract authoring, e-sign, renewal alerts, and obligation tracking.", ProjectStatus.Planning, 93000, 2026, 7, 1, 2027, 1, 31),
        P("Procurement Portal", "PRJ-PRP-021", "RFP management, vendor bidding, and purchase requisition workflows.", ProjectStatus.Active, 81000, 2026, 3, 20, 2026, 9, 30),
        P("Supplier Risk Assessment", "PRJ-SUP-022", "Vendor scorecards, compliance checks, and risk monitoring dashboards.", ProjectStatus.OnHold, 48000, 2026, 5, 1, 2026, 11, 30),
        P("Quality Assurance Dashboard", "PRJ-QLT-023", "Defect tracking, inspection workflows, and supplier quality metrics.", ProjectStatus.Active, 59000, 2026, 2, 1, 2026, 8, 31),
        P("Manufacturing Execution System", "PRJ-MFG-024", "Shop floor scheduling, work orders, and production tracking.", ProjectStatus.Active, 245000, 2026, 1, 1, 2027, 6, 30),
        P("IoT Sensor Integration", "PRJ-IOT-025", "Connect factory sensors for real-time equipment monitoring.", ProjectStatus.Planning, 134000, 2026, 8, 1, 2027, 4, 30),
        P("Blockchain Audit Trail", "PRJ-BLK-026", "Immutable ledger for supply chain and financial audit events.", ProjectStatus.Planning, 102000, 2026, 9, 1, 2027, 5, 31),
        P("AI Demand Forecasting", "PRJ-AIM-027", "Predictive models for sales and inventory planning.", ProjectStatus.Active, 156000, 2026, 4, 15, 2026, 12, 31),
        P("Customer Health Scoring", "PRJ-CHT-028", "Churn risk scoring and account health dashboards for CS teams.", ProjectStatus.Active, 72000, 2026, 3, 1, 2026, 9, 15),
        P("Revenue Recognition Module", "PRJ-REV-029", "ASC 606 compliant revenue scheduling and deferral tracking.", ProjectStatus.Active, 118000, 2026, 2, 10, 2026, 10, 31),
        P("Global Tax Compliance", "PRJ-TAX-030", "Multi-jurisdiction tax calculation and filing automation.", ProjectStatus.Planning, 89000, 2026, 7, 15, 2027, 2, 28),
        P("Fleet Management System", "PRJ-FLT-031", "Vehicle tracking, maintenance schedules, and fuel cost analytics.", ProjectStatus.Active, 63000, 2026, 5, 1, 2026, 11, 30),
        P("Legal Matter Management", "PRJ-LEG-032", "Case tracking, document management, and outside counsel billing.", ProjectStatus.OnHold, 55000, 2026, 6, 1, 2026, 12, 31),
        P("R&D Portfolio Tracker", "PRJ-RND-033", "Innovation pipeline, patent tracking, and R&D budget allocation.", ProjectStatus.Active, 84000, 2026, 1, 20, 2026, 12, 31),
        P("E-Commerce Integration", "PRJ-ECO-034", "Shopify and Amazon marketplace order and inventory sync.", ProjectStatus.Active, 97000, 2026, 4, 1, 2026, 10, 31),
        P("Point of Sale Rollout", "PRJ-POS-035", "Deploy POS terminals across 50 retail locations.", ProjectStatus.Planning, 178000, 2026, 8, 1, 2027, 3, 31),
        P("Loyalty Program Platform", "PRJ-LOY-036", "Points, tiers, rewards catalog, and member engagement campaigns.", ProjectStatus.Active, 68000, 2026, 3, 15, 2026, 9, 30),
        P("M&A Integration Playbook", "PRJ-MAF-037", "Post-merger systems integration checklist and tracking.", ProjectStatus.Completed, 42000, 2025, 9, 1, 2026, 3, 31),
        P("Governance Risk Compliance", "PRJ-GRC-038", "Policy management, control testing, and regulatory reporting.", ProjectStatus.Active, 105000, 2026, 2, 1, 2026, 11, 30),
        P("Document Retention Policy", "PRJ-DRP-039", "Automated retention schedules and legal hold management.", ProjectStatus.Active, 46000, 2026, 4, 15, 2026, 8, 31),
        P("Business Continuity Plan", "PRJ-BCP-040", "Disaster recovery runbooks and annual continuity drills.", ProjectStatus.Planning, 38000, 2026, 9, 1, 2027, 1, 31),
        P("Identity Federation SSO", "PRJ-IDS-041", "SAML/OIDC SSO for all internal and partner applications.", ProjectStatus.Active, 74000, 2026, 3, 1, 2026, 8, 15),
        P("Network Operations Center", "PRJ-NOC-042", "24/7 network monitoring, alerting, and incident response.", ProjectStatus.Active, 128000, 2026, 1, 15, 2026, 10, 31),
        P("Security Operations Center", "PRJ-SOC-043", "SIEM deployment, threat detection, and incident playbooks.", ProjectStatus.Active, 165000, 2026, 2, 1, 2026, 12, 31),
        P("Customer Data Platform", "PRJ-CDP-044", "Unified customer profiles from CRM, web, and support channels.", ProjectStatus.Planning, 142000, 2026, 7, 1, 2027, 4, 30),
    ];

    internal static Guid? ManagerForCode(string code, Func<string[], Guid?> managerFor)
    {
        return code switch
        {
            "PRJ-FIN-015" or "PRJ-REV-029" or "PRJ-TAX-030" or "PRJ-EXP-018" => managerFor(["robert.finance@nexuserp.com"]),
            "PRJ-INV-016" or "PRJ-MFG-024" or "PRJ-QLT-023" or "PRJ-WMS-013" => managerFor(["sarah.manager@nexuserp.com"]),
            "PRJ-PAY-017" or "PRJ-HR-006" or "PRJ-TRN-009" => managerFor(["priya.lead@nexuserp.com"]),
            "PRJ-ASG-019" or "PRJ-IOT-025" or "PRJ-NOC-042" or "PRJ-INF-010" => managerFor(["alex.devops@nexuserp.com"]),
            "PRJ-CON-020" or "PRJ-LEG-032" or "PRJ-DRP-039" => managerFor(["priya.lead@nexuserp.com", "sarah.manager@nexuserp.com"]),
            "PRJ-PRP-021" or "PRJ-SUP-022" or "PRJ-VND-014" => managerFor(["kate.support@nexuserp.com"]),
            "PRJ-BLK-026" or "PRJ-AIM-027" or "PRJ-CDP-044" => managerFor(["james.lead@nexuserp.com"]),
            "PRJ-CHT-028" or "PRJ-CRM-011" or "PRJ-ECO-034" or "PRJ-LOY-036" => managerFor(["james.lead@nexuserp.com", "sarah.manager@nexuserp.com"]),
            "PRJ-FLT-031" or "PRJ-POS-035" => managerFor(["sarah.manager@nexuserp.com"]),
            "PRJ-RND-033" or "PRJ-MAF-037" => managerFor(["priya.lead@nexuserp.com"]),
            "PRJ-GRC-038" or "PRJ-BCP-040" or "PRJ-SEC-005" => managerFor(["alex.devops@nexuserp.com", "sarah.manager@nexuserp.com"]),
            "PRJ-IDS-041" or "PRJ-SOC-043" => managerFor(["alex.devops@nexuserp.com"]),
            _ => managerFor(["admin@nexuserp.com"])
        };
    }

    internal static void SeedTasks(SeedTaskAdder add)
    {
        // Accounts Payable
        add("PRJ-FIN-015", "Invoice OCR pipeline", "Extract line items from scanned invoices.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 6, 15), 32, 18, "robert.finance@nexuserp.com");
        add("PRJ-FIN-015", "Approval workflow engine", "Multi-level approval routing by amount.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 7, 30), 40, assignee: "emily.dev@nexuserp.com");
        add("PRJ-FIN-015", "Payment batch scheduling", "ACH and wire payment batch generation.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 8, 31), 28, assignee: "anna.member@nexuserp.com");
        add("PRJ-FIN-015", "Vendor payment terms sync", "Sync terms from vendor master records.", TaskStatus.Done, TaskPriority.Medium, 3, D(2026, 5, 1), 16, 15, "robert.finance@nexuserp.com");
        add("PRJ-FIN-015", "Duplicate invoice detection", "Flag potential duplicate submissions.", TaskStatus.InReview, TaskPriority.High, 4, D(2026, 6, 30), 20, 14, "michael.dev@nexuserp.com");

        // Inventory Forecasting
        add("PRJ-INV-016", "Historical data ingestion", "Load 3 years of sales and stock data.", TaskStatus.Done, TaskPriority.Critical, 0, D(2026, 4, 1), 24, 22, "sarah.manager@nexuserp.com");
        add("PRJ-INV-016", "Forecast model training", "Train ARIMA and gradient boosting models.", TaskStatus.InProgress, TaskPriority.Critical, 1, D(2026, 7, 15), 56, 30, "michael.dev@nexuserp.com");
        add("PRJ-INV-016", "Reorder point calculator", "Auto-calculate safety stock and reorder levels.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 9, 1), 32, assignee: "emily.dev@nexuserp.com");
        add("PRJ-INV-016", "Forecast accuracy dashboard", "MAPE and bias metrics by SKU category.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 10, 15), 24, assignee: "anna.member@nexuserp.com");
        add("PRJ-INV-016", "Seasonality adjustment module", "Handle holiday and promotional spikes.", TaskStatus.InReview, TaskPriority.High, 4, D(2026, 8, 1), 28, 16, "tom.developer@nexuserp.com");

        // Payroll
        add("PRJ-PAY-017", "Tax jurisdiction mapping", "Map employees to federal, state, and local rules.", TaskStatus.Todo, TaskPriority.Critical, 0, D(2026, 8, 15), 40, assignee: "robert.finance@nexuserp.com");
        add("PRJ-PAY-017", "Direct deposit integration", "Bank file generation and NACHA compliance.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 9, 30), 32, assignee: "alex.devops@nexuserp.com");
        add("PRJ-PAY-017", "Payslip self-service portal", "Employees view and download pay stubs.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 10, 31), 24, assignee: "tom.developer@nexuserp.com");
        add("PRJ-PAY-017", "Benefits deduction sync", "Sync health and retirement deductions.", TaskStatus.Todo, TaskPriority.High, 3, D(2026, 11, 15), 28, assignee: "priya.lead@nexuserp.com");

        // Expense Management
        add("PRJ-EXP-018", "Mobile receipt scanner", "Camera capture with OCR for expense lines.", TaskStatus.Done, TaskPriority.High, 0, D(2026, 4, 30), 36, 34, "emily.dev@nexuserp.com");
        add("PRJ-EXP-018", "Policy rule engine", "Enforce per-diem and category limits.", TaskStatus.InProgress, TaskPriority.High, 1, D(2026, 6, 30), 32, 20, "tom.developer@nexuserp.com");
        add("PRJ-EXP-018", "Manager approval queue", "Batch approve team expense reports.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 7, 31), 20, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-EXP-018", "Corporate card reconciliation", "Match card transactions to expense reports.", TaskStatus.Todo, TaskPriority.High, 3, D(2026, 8, 15), 28, assignee: "robert.finance@nexuserp.com");

        // Asset Tracking
        add("PRJ-ASG-019", "RFID reader integration", "Connect handheld and fixed RFID readers.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 7, 1), 40, 22, "alex.devops@nexuserp.com");
        add("PRJ-ASG-019", "Asset lifecycle workflows", "Check-out, transfer, and disposal tracking.", TaskStatus.Todo, TaskPriority.Medium, 1, D(2026, 8, 31), 28, assignee: "anna.member@nexuserp.com");
        add("PRJ-ASG-019", "Depreciation calculator", "Straight-line and declining balance methods.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 9, 30), 24, assignee: "robert.finance@nexuserp.com");
        add("PRJ-ASG-019", "Audit trail reporting", "Full history of asset movements.", TaskStatus.Todo, TaskPriority.Low, 3, D(2026, 10, 15), 16, assignee: "lisa.qa@nexuserp.com");
        add("PRJ-ASG-019", "Barcode label printing", "Generate and print asset labels.", TaskStatus.Done, TaskPriority.Medium, 4, D(2026, 5, 15), 12, 11, "tom.developer@nexuserp.com");

        // Contract Lifecycle
        add("PRJ-CON-020", "Contract template library", "Pre-approved templates by contract type.", TaskStatus.Todo, TaskPriority.High, 0, D(2026, 9, 1), 24, assignee: "priya.lead@nexuserp.com");
        add("PRJ-CON-020", "E-signature workflow", "Multi-party signing with audit trail.", TaskStatus.Todo, TaskPriority.Critical, 1, D(2026, 10, 15), 36, assignee: "emily.dev@nexuserp.com");
        add("PRJ-CON-020", "Renewal alert system", "90/60/30 day renewal notifications.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 11, 30), 20, assignee: "anna.member@nexuserp.com");
        add("PRJ-CON-020", "Obligation tracking matrix", "Track deliverables and milestones per contract.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 12, 15), 28, assignee: "sarah.manager@nexuserp.com");

        // Procurement Portal
        add("PRJ-PRP-021", "RFP creation wizard", "Guided RFP builder with evaluation criteria.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 6, 30), 32, 16, "kate.support@nexuserp.com");
        add("PRJ-PRP-021", "Vendor bid comparison", "Side-by-side bid analysis and scoring.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 8, 1), 28, assignee: "robert.finance@nexuserp.com");
        add("PRJ-PRP-021", "Purchase requisition flow", "Request, approve, and convert to PO.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2026, 7, 15), 40, assignee: "emily.dev@nexuserp.com");
        add("PRJ-PRP-021", "Catalog management", "Preferred vendor catalogs and punch-out.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 9, 15), 24, assignee: "anna.member@nexuserp.com");
        add("PRJ-PRP-021", "Spend analytics dashboard", "Category spend trends and savings tracking.", TaskStatus.InReview, TaskPriority.Medium, 4, D(2026, 8, 31), 20, 12, "michael.dev@nexuserp.com");

        // Supplier Risk
        add("PRJ-SUP-022", "Vendor scorecard framework", "Define KPIs for delivery, quality, and cost.", TaskStatus.Todo, TaskPriority.Medium, 0, D(2026, 8, 1), 20, assignee: "kate.support@nexuserp.com");
        add("PRJ-SUP-022", "Compliance document tracking", "Track ISO, SOC2, and insurance certificates.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 9, 15), 28, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-SUP-022", "Risk heat map dashboard", "Visual risk matrix by vendor and category.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 10, 31), 24, assignee: "anna.member@nexuserp.com");

        // Quality Assurance
        add("PRJ-QLT-023", "Defect logging module", "Capture defects with photos and root cause.", TaskStatus.Done, TaskPriority.High, 0, D(2026, 4, 15), 24, 22, "lisa.qa@nexuserp.com");
        add("PRJ-QLT-023", "Inspection checklist builder", "Configurable checklists per product line.", TaskStatus.InProgress, TaskPriority.High, 1, D(2026, 6, 30), 32, 18, "tom.developer@nexuserp.com");
        add("PRJ-QLT-023", "Supplier quality metrics", "PPM, CAPA, and audit score tracking.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 8, 1), 28, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-QLT-023", "Non-conformance workflow", "NCR creation, investigation, and closure.", TaskStatus.Todo, TaskPriority.High, 3, D(2026, 7, 31), 24, assignee: "anna.member@nexuserp.com");

        // Manufacturing Execution
        add("PRJ-MFG-024", "Work order scheduling", "Gantt-based shop floor scheduling.", TaskStatus.InProgress, TaskPriority.Critical, 0, D(2026, 8, 31), 56, 28, "michael.dev@nexuserp.com");
        add("PRJ-MFG-024", "Bill of materials editor", "Multi-level BOM with revision control.", TaskStatus.InProgress, TaskPriority.Critical, 1, D(2026, 7, 15), 48, 24, "emily.dev@nexuserp.com");
        add("PRJ-MFG-024", "Production yield tracking", "Track output vs input and scrap rates.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 10, 1), 32, assignee: "tom.developer@nexuserp.com");
        add("PRJ-MFG-024", "Machine downtime logging", "Record and analyze equipment downtime.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 11, 15), 24, assignee: "alex.devops@nexuserp.com");
        add("PRJ-MFG-024", "OEE dashboard", "Overall equipment effectiveness metrics.", TaskStatus.Todo, TaskPriority.High, 4, D(2026, 12, 31), 36, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-MFG-024", "Shop floor tablet UI", "Touch-friendly interface for operators.", TaskStatus.InReview, TaskPriority.High, 5, D(2026, 9, 1), 40, 20, "lisa.qa@nexuserp.com");

        // IoT Sensors
        add("PRJ-IOT-025", "MQTT broker setup", "Deploy and secure MQTT message broker.", TaskStatus.Todo, TaskPriority.Critical, 0, D(2026, 10, 1), 32, assignee: "alex.devops@nexuserp.com");
        add("PRJ-IOT-025", "Sensor data ingestion pipeline", "Stream processing for temperature and vibration.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 11, 15), 48, assignee: "michael.dev@nexuserp.com");
        add("PRJ-IOT-025", "Equipment alert thresholds", "Configurable alerts for anomaly detection.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 12, 31), 28, assignee: "emily.dev@nexuserp.com");
        add("PRJ-IOT-025", "Edge gateway firmware update", "OTA updates for edge devices.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2027, 1, 31), 36, assignee: "alex.devops@nexuserp.com");

        // Blockchain Audit
        add("PRJ-BLK-026", "Ledger architecture design", "Choose permissioned blockchain framework.", TaskStatus.Todo, TaskPriority.High, 0, D(2026, 10, 15), 24, assignee: "james.lead@nexuserp.com");
        add("PRJ-BLK-026", "Smart contract prototypes", "PO and shipment event recording contracts.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 12, 1), 40, assignee: "michael.dev@nexuserp.com");
        add("PRJ-BLK-026", "Audit event API", "REST API to write and query audit events.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2027, 1, 15), 32, assignee: "emily.dev@nexuserp.com");

        // AI Demand Forecasting
        add("PRJ-AIM-027", "Feature engineering pipeline", "Build time-series features from sales data.", TaskStatus.Done, TaskPriority.Critical, 0, D(2026, 5, 15), 40, 38, "james.lead@nexuserp.com");
        add("PRJ-AIM-027", "Model training infrastructure", "GPU cluster for batch model training.", TaskStatus.InProgress, TaskPriority.Critical, 1, D(2026, 8, 1), 56, 30, "alex.devops@nexuserp.com");
        add("PRJ-AIM-027", "Prediction API endpoint", "Serve forecasts via REST with caching.", TaskStatus.InProgress, TaskPriority.High, 2, D(2026, 9, 15), 40, 22, "michael.dev@nexuserp.com");
        add("PRJ-AIM-027", "Model monitoring dashboard", "Track drift and prediction accuracy over time.", TaskStatus.Todo, TaskPriority.High, 3, D(2026, 11, 1), 28, assignee: "emily.dev@nexuserp.com");
        add("PRJ-AIM-027", "A/B test framework", "Compare model versions in production.", TaskStatus.Todo, TaskPriority.Medium, 4, D(2026, 12, 15), 24, assignee: "tom.developer@nexuserp.com");

        // Customer Health
        add("PRJ-CHT-028", "Health score algorithm", "Weighted scoring from usage, support, and NPS.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 7, 1), 32, 18, "james.lead@nexuserp.com");
        add("PRJ-CHT-028", "Churn risk alerts", "Notify CSM when account health drops.", TaskStatus.Todo, TaskPriority.Critical, 1, D(2026, 8, 15), 24, assignee: "kate.support@nexuserp.com");
        add("PRJ-CHT-028", "Account health dashboard", "Portfolio view with trend sparklines.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 9, 1), 36, assignee: "emily.dev@nexuserp.com");
        add("PRJ-CHT-028", "Playbook automation", "Trigger outreach tasks on score changes.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 9, 30), 20, assignee: "anna.member@nexuserp.com");

        // Revenue Recognition
        add("PRJ-REV-029", "Performance obligation mapping", "Map contracts to ASC 606 obligations.", TaskStatus.InProgress, TaskPriority.Critical, 0, D(2026, 7, 31), 48, 26, "robert.finance@nexuserp.com");
        add("PRJ-REV-029", "Revenue schedule engine", "Calculate recognized vs deferred amounts.", TaskStatus.InProgress, TaskPriority.Critical, 1, D(2026, 8, 31), 56, 28, "emily.dev@nexuserp.com");
        add("PRJ-REV-029", "Journal entry automation", "Auto-generate GL entries for recognition.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 9, 30), 32, assignee: "robert.finance@nexuserp.com");
        add("PRJ-REV-029", "Disclosure report templates", "Quarterly revenue disclosure reports.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 10, 15), 20, assignee: "anna.member@nexuserp.com");
        add("PRJ-REV-029", "Auditor review package", "Export supporting docs for external audit.", TaskStatus.InReview, TaskPriority.High, 4, D(2026, 10, 31), 16, 10, "lisa.qa@nexuserp.com");

        // Global Tax
        add("PRJ-TAX-030", "Tax rate database integration", "Connect to Vertex/Avalara rate services.", TaskStatus.Todo, TaskPriority.Critical, 0, D(2026, 10, 1), 40, assignee: "robert.finance@nexuserp.com");
        add("PRJ-TAX-030", "VAT/GST calculation engine", "Multi-country indirect tax rules.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 11, 30), 48, assignee: "michael.dev@nexuserp.com");
        add("PRJ-TAX-030", "Filing calendar tracker", "Track deadlines by jurisdiction.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 12, 31), 20, assignee: "anna.member@nexuserp.com");
        add("PRJ-TAX-030", "Transfer pricing documentation", "Generate TP reports for intercompany.", TaskStatus.Todo, TaskPriority.High, 3, D(2027, 1, 31), 32, assignee: "robert.finance@nexuserp.com");

        // Fleet Management
        add("PRJ-FLT-031", "GPS tracker integration", "Real-time vehicle location tracking.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 7, 31), 36, 20, "alex.devops@nexuserp.com");
        add("PRJ-FLT-031", "Maintenance schedule alerts", "Oil change and inspection reminders.", TaskStatus.Todo, TaskPriority.Medium, 1, D(2026, 8, 31), 20, assignee: "anna.member@nexuserp.com");
        add("PRJ-FLT-031", "Fuel cost analytics", "MPG tracking and cost per mile reports.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 9, 30), 24, assignee: "robert.finance@nexuserp.com");
        add("PRJ-FLT-031", "Driver assignment module", "Assign vehicles and track utilization.", TaskStatus.Todo, TaskPriority.Low, 3, D(2026, 10, 15), 16, assignee: "sarah.manager@nexuserp.com");

        // Legal Matter
        add("PRJ-LEG-032", "Case intake form", "Standardized legal matter submission.", TaskStatus.Todo, TaskPriority.Medium, 0, D(2026, 9, 1), 16, assignee: "priya.lead@nexuserp.com");
        add("PRJ-LEG-032", "Outside counsel billing review", "Compare invoices against matter budgets.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 10, 15), 28, assignee: "robert.finance@nexuserp.com");
        add("PRJ-LEG-032", "Matter document repository", "Secure storage with privilege tagging.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 11, 30), 24, assignee: "emily.dev@nexuserp.com");

        // R&D Portfolio
        add("PRJ-RND-033", "Innovation pipeline board", "Kanban for ideas through commercialization.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 7, 15), 28, 14, "priya.lead@nexuserp.com");
        add("PRJ-RND-033", "Patent filing tracker", "Track provisional and full patent applications.", TaskStatus.Todo, TaskPriority.Medium, 1, D(2026, 9, 1), 20, assignee: "anna.member@nexuserp.com");
        add("PRJ-RND-033", "R&D budget allocation", "Allocate budget across projects and teams.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 8, 31), 24, assignee: "robert.finance@nexuserp.com");
        add("PRJ-RND-033", "Research milestone reporting", "Quarterly progress reports for leadership.", TaskStatus.Todo, TaskPriority.Low, 3, D(2026, 10, 31), 12, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-RND-033", "Lab equipment reservation", "Book shared lab equipment and resources.", TaskStatus.Done, TaskPriority.Medium, 4, D(2026, 5, 1), 16, 15, "tom.developer@nexuserp.com");

        // E-Commerce
        add("PRJ-ECO-034", "Shopify order sync", "Real-time order and fulfillment sync.", TaskStatus.InProgress, TaskPriority.Critical, 0, D(2026, 7, 1), 40, 22, "michael.dev@nexuserp.com");
        add("PRJ-ECO-034", "Amazon MWS integration", "FBA inventory and order management.", TaskStatus.InProgress, TaskPriority.High, 1, D(2026, 8, 15), 48, 20, "emily.dev@nexuserp.com");
        add("PRJ-ECO-034", "Unified inventory pool", "Single stock count across all channels.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2026, 9, 30), 36, assignee: "tom.developer@nexuserp.com");
        add("PRJ-ECO-034", "Channel pricing rules", "Different pricing per marketplace.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 10, 15), 24, assignee: "anna.member@nexuserp.com");
        add("PRJ-ECO-034", "Returns processing workflow", "RMA handling across all channels.", TaskStatus.Todo, TaskPriority.High, 4, D(2026, 10, 31), 28, assignee: "kate.support@nexuserp.com");

        // Point of Sale
        add("PRJ-POS-035", "POS hardware procurement", "Select and order terminals for 50 stores.", TaskStatus.Todo, TaskPriority.High, 0, D(2026, 10, 1), 24, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-POS-035", "Terminal software deployment", "Install and configure POS application.", TaskStatus.Todo, TaskPriority.Critical, 1, D(2026, 12, 15), 56, assignee: "alex.devops@nexuserp.com");
        add("PRJ-POS-035", "Payment processor integration", "EMV chip and contactless payment support.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2026, 11, 30), 40, assignee: "michael.dev@nexuserp.com");
        add("PRJ-POS-035", "Store staff training materials", "Training videos and quick reference guides.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2027, 1, 31), 20, assignee: "priya.lead@nexuserp.com");
        add("PRJ-POS-035", "Pilot store rollout", "Deploy to 5 pilot locations first.", TaskStatus.Todo, TaskPriority.High, 4, D(2026, 11, 1), 32, assignee: "lisa.qa@nexuserp.com");

        // Loyalty Program
        add("PRJ-LOY-036", "Points accrual engine", "Calculate points on purchases and actions.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 7, 31), 32, 18, "james.lead@nexuserp.com");
        add("PRJ-LOY-036", "Tier management system", "Bronze, silver, gold tier progression.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 8, 31), 28, assignee: "emily.dev@nexuserp.com");
        add("PRJ-LOY-036", "Rewards catalog UI", "Browse and redeem rewards online.", TaskStatus.Todo, TaskPriority.Medium, 2, D(2026, 9, 15), 36, assignee: "tom.developer@nexuserp.com");
        add("PRJ-LOY-036", "Campaign email integration", "Trigger loyalty emails from marketing.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 9, 30), 20, assignee: "anna.member@nexuserp.com");

        // M&A Integration (completed project)
        add("PRJ-MAF-037", "Systems inventory audit", "Catalog all apps and integrations.", TaskStatus.Done, TaskPriority.Critical, 0, D(2025, 10, 15), 32, 30, "priya.lead@nexuserp.com");
        add("PRJ-MAF-037", "Data migration cutover", "Migrate acquired company data.", TaskStatus.Done, TaskPriority.Critical, 1, D(2026, 1, 31), 56, 54, "alex.devops@nexuserp.com");
        add("PRJ-MAF-037", "Employee onboarding batch", "Onboard 120 acquired employees.", TaskStatus.Done, TaskPriority.High, 2, D(2026, 2, 28), 40, 38, "priya.lead@nexuserp.com");
        add("PRJ-MAF-037", "Brand consolidation", "Unify branding across digital properties.", TaskStatus.Done, TaskPriority.Medium, 3, D(2026, 3, 15), 24, 22, "james.lead@nexuserp.com");

        // GRC
        add("PRJ-GRC-038", "Policy document repository", "Central store for corporate policies.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 7, 1), 24, 12, "sarah.manager@nexuserp.com");
        add("PRJ-GRC-038", "Control testing scheduler", "Annual control test calendar and assignments.", TaskStatus.Todo, TaskPriority.High, 1, D(2026, 8, 31), 32, assignee: "anna.member@nexuserp.com");
        add("PRJ-GRC-038", "Regulatory reporting module", "SOX, GDPR, and industry-specific reports.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2026, 10, 15), 48, assignee: "robert.finance@nexuserp.com");
        add("PRJ-GRC-038", "Risk register dashboard", "Track and score enterprise risks.", TaskStatus.Todo, TaskPriority.High, 3, D(2026, 9, 30), 28, assignee: "alex.devops@nexuserp.com");
        add("PRJ-GRC-038", "Third-party risk assessments", "Vendor security questionnaire workflow.", TaskStatus.InReview, TaskPriority.Medium, 4, D(2026, 8, 1), 20, 14, "lisa.qa@nexuserp.com");

        // Document Retention
        add("PRJ-DRP-039", "Retention schedule builder", "Define retention periods by document type.", TaskStatus.InProgress, TaskPriority.High, 0, D(2026, 6, 30), 28, 16, "priya.lead@nexuserp.com");
        add("PRJ-DRP-039", "Legal hold management", "Suspend deletion for litigation holds.", TaskStatus.Todo, TaskPriority.Critical, 1, D(2026, 7, 31), 32, assignee: "emily.dev@nexuserp.com");
        add("PRJ-DRP-039", "Automated purge jobs", "Scheduled deletion of expired documents.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 8, 15), 24, assignee: "alex.devops@nexuserp.com");
        add("PRJ-DRP-039", "Compliance audit report", "Prove retention policy adherence.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 8, 31), 16, assignee: "lisa.qa@nexuserp.com");

        // Business Continuity
        add("PRJ-BCP-040", "BIA questionnaire rollout", "Business impact analysis for all departments.", TaskStatus.Todo, TaskPriority.High, 0, D(2026, 11, 1), 24, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-BCP-040", "DR runbook documentation", "Step-by-step recovery procedures.", TaskStatus.Todo, TaskPriority.Critical, 1, D(2026, 12, 15), 40, assignee: "alex.devops@nexuserp.com");
        add("PRJ-BCP-040", "Annual continuity drill", "Simulate datacenter failover exercise.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2027, 1, 15), 32, assignee: "alex.devops@nexuserp.com");
        add("PRJ-BCP-040", "Crisis communication plan", "Notification tree and status page setup.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 11, 30), 16, assignee: "kate.support@nexuserp.com");

        // Identity Federation
        add("PRJ-IDS-041", "SAML IdP configuration", "Configure SAML for enterprise SSO.", TaskStatus.InProgress, TaskPriority.Critical, 0, D(2026, 6, 30), 40, 24, "alex.devops@nexuserp.com");
        add("PRJ-IDS-041", "OIDC client registration", "Register all internal apps as OIDC clients.", TaskStatus.InProgress, TaskPriority.High, 1, D(2026, 7, 15), 32, 18, "michael.dev@nexuserp.com");
        add("PRJ-IDS-041", "MFA enforcement policy", "Require MFA for all federated logins.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2026, 8, 1), 24, assignee: "alex.devops@nexuserp.com");
        add("PRJ-IDS-041", "Partner SSO onboarding", "Self-service partner IdP registration.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 8, 31), 28, assignee: "emily.dev@nexuserp.com");
        add("PRJ-IDS-041", "Session management dashboard", "View and revoke active user sessions.", TaskStatus.Todo, TaskPriority.High, 4, D(2026, 8, 15), 20, assignee: "tom.developer@nexuserp.com");

        // NOC
        add("PRJ-NOC-042", "Network monitoring setup", "Deploy SNMP and flow collectors.", TaskStatus.Done, TaskPriority.Critical, 0, D(2026, 4, 1), 48, 45, "alex.devops@nexuserp.com");
        add("PRJ-NOC-042", "Alert escalation matrix", "Define on-call rotation and escalation.", TaskStatus.InProgress, TaskPriority.High, 1, D(2026, 6, 30), 20, 12, "alex.devops@nexuserp.com");
        add("PRJ-NOC-042", "NOC dashboard wallboard", "Real-time status display for NOC team.", TaskStatus.InProgress, TaskPriority.Medium, 2, D(2026, 7, 31), 32, 16, "tom.developer@nexuserp.com");
        add("PRJ-NOC-042", "Incident postmortem template", "Standardized incident review process.", TaskStatus.Todo, TaskPriority.Medium, 3, D(2026, 8, 15), 12, assignee: "sarah.manager@nexuserp.com");
        add("PRJ-NOC-042", "Bandwidth utilization reports", "Monthly capacity planning reports.", TaskStatus.Todo, TaskPriority.Low, 4, D(2026, 9, 1), 16, assignee: "anna.member@nexuserp.com");

        // SOC
        add("PRJ-SOC-043", "SIEM deployment", "Deploy and tune SIEM correlation rules.", TaskStatus.InProgress, TaskPriority.Critical, 0, D(2026, 8, 1), 64, 36, "alex.devops@nexuserp.com");
        add("PRJ-SOC-043", "Threat intel feed integration", "Connect commercial and OSINT feeds.", TaskStatus.InProgress, TaskPriority.High, 1, D(2026, 9, 15), 40, 20, "alex.devops@nexuserp.com");
        add("PRJ-SOC-043", "Incident response playbooks", "Documented playbooks for top 10 threats.", TaskStatus.Todo, TaskPriority.Critical, 2, D(2026, 10, 1), 32, assignee: "alex.devops@nexuserp.com");
        add("PRJ-SOC-043", "SOC analyst training", "Train 4 analysts on SIEM and tools.", TaskStatus.Todo, TaskPriority.High, 3, D(2026, 9, 30), 24, assignee: "priya.lead@nexuserp.com");
        add("PRJ-SOC-043", "Vulnerability scanning automation", "Weekly automated vuln scans.", TaskStatus.Todo, TaskPriority.High, 4, D(2026, 10, 31), 28, assignee: "lisa.qa@nexuserp.com");
        add("PRJ-SOC-043", "Pen test remediation tracking", "Track findings from annual pen test.", TaskStatus.InReview, TaskPriority.Critical, 5, D(2026, 11, 15), 24, 14, "alex.devops@nexuserp.com");

        // Customer Data Platform
        add("PRJ-CDP-044", "Data source connectors", "Ingest from CRM, web, email, and support.", TaskStatus.Todo, TaskPriority.Critical, 0, D(2026, 10, 1), 56, assignee: "james.lead@nexuserp.com");
        add("PRJ-CDP-044", "Identity resolution engine", "Match records across channels to one profile.", TaskStatus.Todo, TaskPriority.Critical, 1, D(2026, 11, 15), 48, assignee: "michael.dev@nexuserp.com");
        add("PRJ-CDP-044", "Unified profile API", "Single API for customer 360 view.", TaskStatus.Todo, TaskPriority.High, 2, D(2026, 12, 31), 40, assignee: "emily.dev@nexuserp.com");
        add("PRJ-CDP-044", "Segmentation builder UI", "Create audience segments with drag-and-drop.", TaskStatus.Todo, TaskPriority.High, 3, D(2027, 1, 31), 36, assignee: "tom.developer@nexuserp.com");
        add("PRJ-CDP-044", "Privacy consent management", "GDPR consent tracking and preferences.", TaskStatus.Todo, TaskPriority.Critical, 4, D(2027, 2, 28), 28, assignee: "anna.member@nexuserp.com");
    }

    private static Project P(string name, string code, string description, ProjectStatus status,
        decimal budget, int startYear, int startMonth, int startDay, int endYear, int endMonth, int endDay) =>
        new()
        {
            Name = name,
            Code = code,
            Description = description,
            Status = status,
            Budget = budget,
            StartDate = new DateTime(startYear, startMonth, startDay, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(endYear, endMonth, endDay, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "system"
        };

    private static DateTime D(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    internal delegate void SeedTaskAdder(
        string projectCode, string title, string? description, TaskStatus status,
        TaskPriority priority, int order, DateTime? due, decimal? estHours,
        decimal? actualHours = null, string assignee = "admin@nexuserp.com");
}
