USE [scriptDB]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Carriers]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Carriers]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Clients]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Clients]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Coverages]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Coverages]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_MasterIncomingPaymentTypes]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_MasterIncomingPaymentTypes]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_MasterPolicyModes]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_MasterPolicyModes]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_MasterPostStatus]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_MasterPostStatus]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Payors]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Payors]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Policies]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Policies]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Statements]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Statements]
GO
/****** Object:  Table [dbo].[EntriesByDEU]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Carriers]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Clients]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Coverages]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_MasterIncomingPaymentTypes]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_MasterPolicyModes]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_MasterPostStatus]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Payors]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Policies]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [FK_EntriesByDEU_Statements]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [DF_EntriesByDEU_DEUEntryID]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [DF_EntriesByDEU_CommissionTotal]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [DF_EntriesByDEU_IsEntrybyCommissiondashBoard]
GO
ALTER TABLE [dbo].[EntriesByDEU] DROP CONSTRAINT [DF_EntriesByDEU_PostCompleteStatus]
GO
DROP TABLE [dbo].[EntriesByDEU]
GO
/****** Object:  Table [dbo].[EntriesByDEU]    Script Date: 02/22/2012 11:29:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EntriesByDEU](
	[DEUEntryID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_EntriesByDEU_DEUEntryID]  DEFAULT (newid()),
	[OriginalEffectiveDate] [datetime] NULL,
	[PaymentReceived] [money] NULL,
	[CommissionPercentage] [float] NULL,
	[OtherData] [xml] NULL,
	[Insured] [varchar](50) NULL,
	[PolicyNumber] [varchar](50) NULL,
	[Enrolled] [varchar](50) NULL,
	[Eligible] [varchar](50) NULL,
	[Link1] [varchar](50) NULL,
	[SplitPer] [float] NULL,
	[PolicyModeID] [int] NULL,
	[PolicyModeValue] [nvarchar](50) NULL,
	[TrackFromDate] [datetime] NULL,
	[CompTypeID] [int] NULL,
	[ClientID] [uniqueidentifier] NULL,
	[StatementID] [uniqueidentifier] NULL,
	[PostStatusID] [int] NULL,
	[PolicyID] [uniqueidentifier] NULL,
	[InvoiceDate] [datetime] NULL,
	[PayorId] [uniqueidentifier] NULL,
	[NumberOfUnits] [int] NULL,
	[DollerPerUnit] [money] NULL,
	[Fee] [money] NULL,
	[Bonus] [money] NULL,
	[CommissionTotal] [money] NULL CONSTRAINT [DF_EntriesByDEU_CommissionTotal]  DEFAULT ((0)),
	[PayorSysID] [nvarchar](50) NULL,
	[Renewal] [nvarchar](50) NULL,
	[CarrierId] [uniqueidentifier] NULL,
	[CoverageId] [uniqueidentifier] NULL,
	[IsEntrybyCommissiondashBoard] [bit] NULL CONSTRAINT [DF_EntriesByDEU_IsEntrybyCommissiondashBoard]  DEFAULT ((0)),
	[CreatedBy] [uniqueidentifier] NULL,
	[PostCompleteStatus] [int] NULL CONSTRAINT [DF_EntriesByDEU_PostCompleteStatus]  DEFAULT ((0)),
	[ModalAvgPremium] [varchar](50) NULL,
	[CompScheduleType] [varchar](50) NULL,
	[ClientValue] [nvarchar](50) NULL,
	[CarrierName] [nvarchar](150) NULL,
	[ProductName] [nvarchar](150) NULL,
	[EntryDate] [datetime] NULL,
 CONSTRAINT [PK_EntriesByDEU] PRIMARY KEY CLUSTERED 
(
	[DEUEntryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Head(Lives or subs)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EntriesByDEU', @level2type=N'COLUMN',@level2name=N'NumberOfUnits'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Dollar per head' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EntriesByDEU', @level2type=N'COLUMN',@level2name=N'DollerPerUnit'
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Carriers]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_Carriers] FOREIGN KEY([CarrierId])
REFERENCES [dbo].[Carriers] ([CarrierId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_Carriers]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Clients]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_Clients] FOREIGN KEY([ClientID])
REFERENCES [dbo].[Clients] ([ClientId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_Clients]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Coverages]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_Coverages] FOREIGN KEY([CoverageId])
REFERENCES [dbo].[Coverages] ([CoverageId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_Coverages]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_MasterIncomingPaymentTypes]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_MasterIncomingPaymentTypes] FOREIGN KEY([CompTypeID])
REFERENCES [dbo].[MasterIncomingPaymentTypes] ([IncomingPaymentTypeId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_MasterIncomingPaymentTypes]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_MasterPolicyModes]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_MasterPolicyModes] FOREIGN KEY([PolicyModeID])
REFERENCES [dbo].[MasterPolicyModes] ([PolicyModeId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_MasterPolicyModes]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_MasterPostStatus]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_MasterPostStatus] FOREIGN KEY([PostStatusID])
REFERENCES [dbo].[MasterPostStatus] ([PostStatusID])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_MasterPostStatus]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Payors]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_Payors] FOREIGN KEY([PayorId])
REFERENCES [dbo].[Payors] ([PayorId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_Payors]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Policies]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_Policies] FOREIGN KEY([PolicyID])
REFERENCES [dbo].[Policies] ([PolicyId])
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_Policies]
GO
/****** Object:  ForeignKey [FK_EntriesByDEU_Statements]    Script Date: 02/22/2012 11:29:09 ******/
ALTER TABLE [dbo].[EntriesByDEU]  WITH NOCHECK ADD  CONSTRAINT [FK_EntriesByDEU_Statements] FOREIGN KEY([StatementID])
REFERENCES [dbo].[Statements] ([StatementId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EntriesByDEU] CHECK CONSTRAINT [FK_EntriesByDEU_Statements]
GO
