Create PROCEDURE [dbo].[SpNAuditLeadStatusTransitionTrigger]
	@StampUser varchar (255),
	@LeadStatusTransitionTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TLeadStatusTransitionTriggerAudit 
( LeadStatusTransitionTriggerId, LeadStatusFromId, LeadStatusToId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select LeadStatusTransitionTriggerId, LeadStatusFromId, LeadStatusToId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TLeadStatusTransitionTrigger
WHERE LeadStatusTransitionTriggerId = @LeadStatusTransitionTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)