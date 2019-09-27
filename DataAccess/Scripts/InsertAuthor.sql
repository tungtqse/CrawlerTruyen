USE [CrawlerTruyenCV]
GO

INSERT INTO [dbo].[Author]
           ([Id]
           ,[Name]
           ,[CreatedBy]
           ,[ModifiedBy]
           ,[CreatedDate]
           ,[ModifiedDate]
           ,[StatusId]
           ,[Link])
     VALUES
           (NEWID()
           ,N'Thiên Sơn Hàn Nha'
           ,'DFD60091-FD1B-416C-A390-1C829B1152D8'
           ,'DFD60091-FD1B-416C-A390-1C829B1152D8'
           ,GETDATE()
           ,GETDATE()
           ,1
           ,'https://wikidich.com/tac-gia/%E5%A4%A9%E5%B1%B1%E5%AF%92%E9%B8%A6')
GO


