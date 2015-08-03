Create PROCEDURE [dbo].[SpNAuditLeadInterestTrigger]
	@StampUser varchar (255),
	@LeadInterestTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TLeadInterestTriggerAudit 
(LeadInterestTriggerId, RefInterestId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select LeadInterestTriggerId, RefInterestId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TLeadInterestTrigger
WHERE LeadInterestTriggerId = @LeadInterestTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)