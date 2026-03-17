// ============================================================
//  Paralogamadha.Data / Repositories / SongsRepository.cs
// ============================================================
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Paralogamadha.Core.Interfaces;
using Paralogamadha.Core.Models;
using Paralogamadha.Data.Infrastructure;

namespace Paralogamadha.Data.Repositories
{
    public interface ISongsRepository
    {
        IEnumerable<SongCategory> GetCategories(int? languageId);
        IEnumerable<Song>         GetAll(int? languageId);
        IEnumerable<Song>         GetByCategory(int categoryId, int languageId);
        Song                      GetById(int id);
        int                       Upsert(Song song);
        bool                      Delete(int id);
    }

    public class SongsRepository : BaseRepository, ISongsRepository
    {
        public IEnumerable<SongCategory> GetCategories(int? languageId)
        {
            using var c = CreateConnection();
            return c.Query<SongCategory>(
                @"SELECT * FROM cms.SongCategories
                  WHERE IsActive=1 AND (@LangId IS NULL OR LanguageId=@LangId)
                  ORDER BY SortOrder",
                new { LangId = languageId });
        }

        public IEnumerable<Song> GetAll(int? languageId)
        {
            using var c = CreateConnection();
            return c.Query<Song>(
                @"SELECT s.*, sc.CategoryName FROM cms.Songs s
                  INNER JOIN cms.SongCategories sc ON sc.CategoryId = s.CategoryId
                  WHERE s.IsDeleted=0 AND s.IsPublished=1
                  AND (@LangId IS NULL OR s.LanguageId=@LangId)
                  ORDER BY sc.SortOrder, s.SortOrder",
                new { LangId = languageId });
        }

        public IEnumerable<Song> GetByCategory(int categoryId, int languageId)
        {
            using var c = CreateConnection();
            return c.Query<Song>(
                @"SELECT s.*, sc.CategoryName FROM cms.Songs s
                  INNER JOIN cms.SongCategories sc ON sc.CategoryId = s.CategoryId
                  WHERE s.IsDeleted=0 AND s.IsPublished=1
                  AND s.CategoryId=@CategoryId
                  AND (s.LanguageId=@LangId OR s.LanguageId=1)
                  ORDER BY s.SortOrder",
                new { CategoryId = categoryId, LangId = languageId });
        }

        public Song GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Song>(
                "SELECT * FROM cms.Songs WHERE SongId=@Id AND IsDeleted=0", new { Id = id });
        }

        public int Upsert(Song s)
        {
            using var c = CreateConnection();
            if (s.SongId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.Songs (CategoryId, LanguageId, Title, Lyrics, AudioFileUrl, Duration, SortOrder, IsPublished, CreatedBy)
                    OUTPUT INSERTED.SongId
                    VALUES (@CategoryId,@LanguageId,@Title,@Lyrics,@AudioFileUrl,@Duration,@SortOrder,@IsPublished,@CreatedBy)", s);
            }
            c.Execute(@"UPDATE cms.Songs SET CategoryId=@CategoryId, LanguageId=@LanguageId,
                Title=@Title, Lyrics=@Lyrics, AudioFileUrl=@AudioFileUrl, Duration=@Duration,
                SortOrder=@SortOrder, IsPublished=@IsPublished, UpdatedAt=GETUTCDATE()
                WHERE SongId=@SongId", s);
            return s.SongId;
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.Songs SET IsDeleted=1 WHERE SongId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Videos Repository ────────────────────────────────────
    public interface IVideosRepository
    {
        IEnumerable<VideoCategory> GetCategories();
        IEnumerable<Video>         GetAll(int? languageId);
        IEnumerable<Video>         GetByCategory(int categoryId, int languageId);
        Video                      GetById(int id);
        int                        Upsert(Video video);
        bool                       Delete(int id);
    }

    public class VideosRepository : BaseRepository, IVideosRepository
    {
        public IEnumerable<VideoCategory> GetCategories()
        {
            using var c = CreateConnection();
            return c.Query<VideoCategory>(
                "SELECT * FROM cms.VideoCategories WHERE IsActive=1 ORDER BY SortOrder");
        }

        public IEnumerable<Video> GetAll(int? languageId)
        {
            using var c = CreateConnection();
            return c.Query<Video>(
                @"SELECT v.*, vc.CategoryName FROM cms.Videos v
                  INNER JOIN cms.VideoCategories vc ON vc.CategoryId = v.CategoryId
                  WHERE v.IsDeleted=0 AND v.IsPublished=1
                  AND (@LangId IS NULL OR v.LanguageId=@LangId)
                  ORDER BY vc.SortOrder, v.SortOrder",
                new { LangId = languageId });
        }

        public IEnumerable<Video> GetByCategory(int categoryId, int languageId)
        {
            using var c = CreateConnection();
            return c.Query<Video>(
                @"SELECT v.*, vc.CategoryName FROM cms.Videos v
                  INNER JOIN cms.VideoCategories vc ON vc.CategoryId = v.CategoryId
                  WHERE v.IsDeleted=0 AND v.IsPublished=1
                  AND v.CategoryId=@CategoryId AND (v.LanguageId=@LangId OR v.LanguageId=1)
                  ORDER BY v.SortOrder",
                new { CategoryId = categoryId, LangId = languageId });
        }

        public Video GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<Video>(
                "SELECT * FROM cms.Videos WHERE VideoId=@Id AND IsDeleted=0", new { Id = id });
        }

        public int Upsert(Video v)
        {
            using var c = CreateConnection();
            if (v.VideoId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.Videos (CategoryId,LanguageId,Title,Description,EmbedType,EmbedCode,ThumbnailUrl,IsPublished,SortOrder,CreatedBy)
                    OUTPUT INSERTED.VideoId
                    VALUES (@CategoryId,@LanguageId,@Title,@Description,@EmbedType,@EmbedCode,@ThumbnailUrl,@IsPublished,@SortOrder,@CreatedBy)", v);
            }
            c.Execute(@"UPDATE cms.Videos SET CategoryId=@CategoryId, LanguageId=@LanguageId,
                Title=@Title, Description=@Description, EmbedType=@EmbedType, EmbedCode=@EmbedCode,
                ThumbnailUrl=@ThumbnailUrl, IsPublished=@IsPublished, SortOrder=@SortOrder,
                UpdatedAt=GETUTCDATE() WHERE VideoId=@VideoId", v);
            return v.VideoId;
        }

        public bool Delete(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.Videos SET IsDeleted=1 WHERE VideoId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Virtual Tour Repository ───────────────────────────────
    public interface IVirtualTourRepository
    {
        IEnumerable<VirtualTourScene> GetAllScenes();
        VirtualTourScene              GetSceneById(int id);
        IEnumerable<TourHotspot>      GetHotspots(int sceneId);
        int                           UpsertScene(VirtualTourScene scene);
        int                           UpsertHotspot(TourHotspot hotspot);
        bool                          DeleteScene(int id);
        bool                          DeleteHotspot(int id);
    }

    public class VirtualTourRepository : BaseRepository, IVirtualTourRepository
    {
        public IEnumerable<VirtualTourScene> GetAllScenes()
        {
            using var c = CreateConnection();
            return c.Query<VirtualTourScene>(
                "SELECT * FROM cms.VirtualTourScenes WHERE IsDeleted=0 AND IsPublished=1 ORDER BY SortOrder");
        }

        public VirtualTourScene GetSceneById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<VirtualTourScene>(
                "SELECT * FROM cms.VirtualTourScenes WHERE SceneId=@Id AND IsDeleted=0", new { Id = id });
        }

        public IEnumerable<TourHotspot> GetHotspots(int sceneId)
        {
            using var c = CreateConnection();
            return c.Query<TourHotspot>(
                "SELECT * FROM cms.TourHotspots WHERE SceneId=@SceneId", new { SceneId = sceneId });
        }

        public int UpsertScene(VirtualTourScene s)
        {
            using var c = CreateConnection();
            if (s.SceneId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.VirtualTourScenes (SceneName,Description,PanoramaImageUrl,ThumbnailUrl,SortOrder,IsPublished)
                    OUTPUT INSERTED.SceneId
                    VALUES (@SceneName,@Description,@PanoramaImageUrl,@ThumbnailUrl,@SortOrder,@IsPublished)", s);
            }
            c.Execute(@"UPDATE cms.VirtualTourScenes SET SceneName=@SceneName, Description=@Description,
                PanoramaImageUrl=@PanoramaImageUrl, ThumbnailUrl=@ThumbnailUrl,
                SortOrder=@SortOrder, IsPublished=@IsPublished WHERE SceneId=@SceneId", s);
            return s.SceneId;
        }

        public int UpsertHotspot(TourHotspot h)
        {
            using var c = CreateConnection();
            if (h.HotspotId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.TourHotspots (SceneId,TargetSceneId,PitchDegree,YawDegree,Label)
                    OUTPUT INSERTED.HotspotId
                    VALUES (@SceneId,@TargetSceneId,@PitchDegree,@YawDegree,@Label)", h);
            }
            c.Execute(@"UPDATE cms.TourHotspots SET TargetSceneId=@TargetSceneId,
                PitchDegree=@PitchDegree, YawDegree=@YawDegree, Label=@Label WHERE HotspotId=@HotspotId", h);
            return h.HotspotId;
        }

        public bool DeleteScene(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.VirtualTourScenes SET IsDeleted=1 WHERE SceneId=@Id", new { Id = id });
            return true;
        }

        public bool DeleteHotspot(int id)
        {
            using var c = CreateConnection();
            c.Execute("DELETE FROM cms.TourHotspots WHERE HotspotId=@Id", new { Id = id });
            return true;
        }
    }

    // ── History Repository ────────────────────────────────────
    public interface IHistoryRepository
    {
        IEnumerable<HistoryContent>  GetContent(int languageId);
        IEnumerable<HistoryTimeline> GetTimeline(int languageId);
        HistoryContent               GetContentById(int id);
        HistoryTimeline              GetTimelineById(int id);
        int                          UpsertContent(HistoryContent content);
        int                          UpsertTimeline(HistoryTimeline item);
        bool                         DeleteContent(int id);
        bool                         DeleteTimeline(int id);
    }

    public class HistoryRepository : BaseRepository, IHistoryRepository
    {
        public IEnumerable<HistoryContent> GetContent(int languageId)
        {
            using var c = CreateConnection();
            return c.Query<HistoryContent>(
                @"SELECT * FROM cms.HistoryContent
                  WHERE IsDeleted=0 AND (LanguageId=@LangId OR LanguageId=1)
                  ORDER BY SortOrder", new { LangId = languageId });
        }

        public IEnumerable<HistoryTimeline> GetTimeline(int languageId)
        {
            using var c = CreateConnection();
            return c.Query<HistoryTimeline>(
                @"SELECT * FROM cms.HistoryTimeline
                  WHERE IsDeleted=0 AND (LanguageId=@LangId OR LanguageId=1)
                  ORDER BY Year ASC", new { LangId = languageId });
        }

        public HistoryContent GetContentById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<HistoryContent>(
                "SELECT * FROM cms.HistoryContent WHERE ContentId=@Id AND IsDeleted=0", new { Id = id });
        }

        public HistoryTimeline GetTimelineById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<HistoryTimeline>(
                "SELECT * FROM cms.HistoryTimeline WHERE TimelineId=@Id AND IsDeleted=0", new { Id = id });
        }

        public int UpsertContent(HistoryContent h)
        {
            using var c = CreateConnection();
            if (h.ContentId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.HistoryContent (LanguageId,Title,BodyHtml,SortOrder,CreatedBy)
                    OUTPUT INSERTED.ContentId
                    VALUES (@LanguageId,@Title,@BodyHtml,@SortOrder,@CreatedBy)", h);
            }
            c.Execute(@"UPDATE cms.HistoryContent SET LanguageId=@LanguageId, Title=@Title,
                BodyHtml=@BodyHtml, SortOrder=@SortOrder, UpdatedAt=GETUTCDATE()
                WHERE ContentId=@ContentId", h);
            return h.ContentId;
        }

        public int UpsertTimeline(HistoryTimeline t)
        {
            using var c = CreateConnection();
            if (t.TimelineId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.HistoryTimeline (LanguageId,Year,Title,Description,ImageUrl,SortOrder)
                    OUTPUT INSERTED.TimelineId
                    VALUES (@LanguageId,@Year,@Title,@Description,@ImageUrl,@SortOrder)", t);
            }
            c.Execute(@"UPDATE cms.HistoryTimeline SET LanguageId=@LanguageId, Year=@Year,
                Title=@Title, Description=@Description, ImageUrl=@ImageUrl, SortOrder=@SortOrder
                WHERE TimelineId=@TimelineId", t);
            return t.TimelineId;
        }

        public bool DeleteContent(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.HistoryContent SET IsDeleted=1 WHERE ContentId=@Id", new { Id = id });
            return true;
        }

        public bool DeleteTimeline(int id)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.HistoryTimeline SET IsDeleted=1 WHERE TimelineId=@Id", new { Id = id });
            return true;
        }
    }

    // ── Live TV Repository ────────────────────────────────────
    public interface ILiveTVRepository
    {
        IEnumerable<LiveTVChannel> GetActive();
        IEnumerable<LiveTVChannel> GetAll();
        LiveTVChannel              GetById(int id);
        int                        Upsert(LiveTVChannel channel);
        void                       ToggleLive(int id, bool isLive, int updatedBy);
    }

    public class LiveTVRepository : BaseRepository, ILiveTVRepository
    {
        public IEnumerable<LiveTVChannel> GetActive()
        {
            using var c = CreateConnection();
            return c.Query<LiveTVChannel>(
                "SELECT * FROM cms.LiveTVChannels WHERE IsActive=1 ORDER BY SortOrder");
        }

        public IEnumerable<LiveTVChannel> GetAll()
        {
            using var c = CreateConnection();
            return c.Query<LiveTVChannel>(
                "SELECT * FROM cms.LiveTVChannels ORDER BY SortOrder");
        }

        public LiveTVChannel GetById(int id)
        {
            using var c = CreateConnection();
            return c.QueryFirstOrDefault<LiveTVChannel>(
                "SELECT * FROM cms.LiveTVChannels WHERE ChannelId=@Id", new { Id = id });
        }

        public int Upsert(LiveTVChannel ch)
        {
            using var c = CreateConnection();
            if (ch.ChannelId == 0)
            {
                return c.QuerySingle<int>(@"
                    INSERT INTO cms.LiveTVChannels (ChannelName,EmbedType,EmbedUrl,IsLive,Schedule,IsActive,SortOrder,UpdatedBy)
                    OUTPUT INSERTED.ChannelId
                    VALUES (@ChannelName,@EmbedType,@EmbedUrl,@IsLive,@Schedule,@IsActive,@SortOrder,@UpdatedBy)", ch);
            }
            c.Execute(@"UPDATE cms.LiveTVChannels SET ChannelName=@ChannelName, EmbedType=@EmbedType,
                EmbedUrl=@EmbedUrl, IsLive=@IsLive, Schedule=@Schedule, IsActive=@IsActive,
                SortOrder=@SortOrder, UpdatedAt=GETUTCDATE(), UpdatedBy=@UpdatedBy WHERE ChannelId=@ChannelId", ch);
            return ch.ChannelId;
        }

        public void ToggleLive(int id, bool isLive, int updatedBy)
        {
            using var c = CreateConnection();
            c.Execute("UPDATE cms.LiveTVChannels SET IsLive=@IsLive, UpdatedAt=GETUTCDATE(), UpdatedBy=@UpdatedBy WHERE ChannelId=@Id",
                new { Id = id, IsLive = isLive, UpdatedBy = updatedBy });
        }
    }
}

// ============================================================
//  Additional Domain Models needed for Phase 3
// ============================================================
namespace Paralogamadha.Core.Models
{
    public class SongCategory
    {
        public int    CategoryId   { get; set; }
        public string CategoryName { get; set; }
        public int    LanguageId   { get; set; }
        public string ColorHex     { get; set; }
        public int    SortOrder    { get; set; }
        public bool   IsActive     { get; set; }
    }

    public class Song
    {
        public int     SongId       { get; set; }
        public int     CategoryId   { get; set; }
        public string  CategoryName { get; set; }
        public int     LanguageId   { get; set; }
        public string  Title        { get; set; }
        public string  Lyrics       { get; set; }
        public string  AudioFileUrl { get; set; }
        public int?    Duration     { get; set; }
        public int     SortOrder    { get; set; }
        public bool    IsPublished  { get; set; }
        public DateTime CreatedAt   { get; set; }
        public int?    CreatedBy    { get; set; }
    }

    public class VideoCategory
    {
        public int    CategoryId   { get; set; }
        public string CategoryName { get; set; }
        public int    SortOrder    { get; set; }
        public bool   IsActive     { get; set; }
    }

    public class Video
    {
        public int    VideoId      { get; set; }
        public int    CategoryId   { get; set; }
        public string CategoryName { get; set; }
        public int    LanguageId   { get; set; }
        public string Title        { get; set; }
        public string Description  { get; set; }
        public string EmbedType    { get; set; }
        public string EmbedCode    { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool   IsPublished  { get; set; }
        public int    SortOrder    { get; set; }
        public int?   CreatedBy    { get; set; }

        public string YouTubeId => EmbedType == "YouTube"
            ? EmbedCode?.Length == 11 ? EmbedCode
              : System.Text.RegularExpressions.Regex.Match(EmbedCode ?? "", @"(?:v=|youtu\.be/)([^&\s]{11})").Groups[1].Value
            : null;
    }

    public class VirtualTourScene
    {
        public int    SceneId          { get; set; }
        public string SceneName        { get; set; }
        public string Description      { get; set; }
        public string PanoramaImageUrl { get; set; }
        public string ThumbnailUrl     { get; set; }
        public int    SortOrder        { get; set; }
        public bool   IsPublished      { get; set; }
        public System.Collections.Generic.IEnumerable<TourHotspot> Hotspots { get; set; }
    }

    public class TourHotspot
    {
        public int     HotspotId     { get; set; }
        public int     SceneId       { get; set; }
        public int     TargetSceneId { get; set; }
        public decimal PitchDegree   { get; set; }
        public decimal YawDegree     { get; set; }
        public string  Label         { get; set; }
    }

    public class HistoryContent
    {
        public int    ContentId  { get; set; }
        public int    LanguageId { get; set; }
        public string Title      { get; set; }
        public string BodyHtml   { get; set; }
        public int    SortOrder  { get; set; }
        public int?   CreatedBy  { get; set; }
    }

    public class HistoryTimeline
    {
        public int    TimelineId  { get; set; }
        public int    LanguageId  { get; set; }
        public int    Year        { get; set; }
        public string Title       { get; set; }
        public string Description { get; set; }
        public string ImageUrl    { get; set; }
        public int    SortOrder   { get; set; }
    }

    public class LiveTVChannel
    {
        public int    ChannelId   { get; set; }
        public string ChannelName { get; set; }
        public string EmbedType   { get; set; }
        public string EmbedUrl    { get; set; }
        public bool   IsLive      { get; set; }
        public string Schedule    { get; set; }
        public bool   IsActive    { get; set; }
        public int    SortOrder   { get; set; }
        public int?   UpdatedBy   { get; set; }
    }
}
