USE [CrawlerTruyenCV]
GO

INSERT INTO [dbo].[Story]
           ([Id]
           ,[Name]
           ,[AuthorId]
           ,[ProgressStatus]
           ,[TotalChapter]
           ,[CreatedBy]
           ,[ModifiedBy]
           ,[CreatedDate]
           ,[ModifiedDate]
           ,[StatusId]
           ,[Link]
		   ,[Source])
     VALUES
           (NEWID()
           ,N'Đại Đường tiểu tướng công'
           ,'085E9F68-D3AE-415F-BFD3-046C1EE3AAFD'
           ,'Processing'
           ,0
           ,'DFD60091-FD1B-416C-A390-1C829B1152D8'
           ,'DFD60091-FD1B-416C-A390-1C829B1152D8'
           ,GETDATE()
           ,GETDATE()
           ,1
           ,'https://truyencv.com/vo-hiep-the-gioi-khong-gian-nang-luc-gia'
		   ,'TruyenCV')





