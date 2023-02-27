
using Newtonsoft.Json;

namespace TeszterTeams
{
	public class Context
	{
		// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
		public record App(
			[property: JsonProperty("locale")] string Locale,
			[property: JsonProperty("sessionId")] string SessionId,
			[property: JsonProperty("theme")] string Theme,
			[property: JsonProperty("iconPositionVertical")] int? IconPositionVertical,
			[property: JsonProperty("parentMessageId")] string ParentMessageId,
			[property: JsonProperty("userClickTime")] long? UserClickTime,
			[property: JsonProperty("userFileOpenPreference")] string UserFileOpenPreference,
			[property: JsonProperty("host")] Host Host,
			[property: JsonProperty("appLaunchId")] string AppLaunchId
		);

		public record Channel(
			[property: JsonProperty("id")] string Id,
			[property: JsonProperty("displayName")] string DisplayName,
			[property: JsonProperty("relativeUrl")] string RelativeUrl,
			[property: JsonProperty("membershipType")] string MembershipType,
			[property: JsonProperty("defaultOneNoteSectionId")] string DefaultOneNoteSectionId
		);

		public record Host(
			[property: JsonProperty("name")] string Name,
			[property: JsonProperty("clientType")] string ClientType,
			[property: JsonProperty("sessionId")] string SessionId,
			[property: JsonProperty("ringId")] string RingId
		);

		public record Page(
			[property: JsonProperty("id")] string Id,
			[property: JsonProperty("frameContext")] string FrameContext,
			[property: JsonProperty("subPageId")] string SubPageId,
			[property: JsonProperty("isFullScreen")] bool? IsFullScreen,
			[property: JsonProperty("isMultiWindow")] bool? IsMultiWindow,
			[property: JsonProperty("sourceOrigin")] object SourceOrigin
		);

		public record Root(
			[property: JsonProperty("app")] App App,
			[property: JsonProperty("page")] Page Page,
			[property: JsonProperty("user")] User User,
			[property: JsonProperty("channel")] Channel Channel,
			[property: JsonProperty("team")] Team Team,
			[property: JsonProperty("sharePointSite")] SharePointSite SharePointSite
		);

		public record SharePointSite(
			[property: JsonProperty("teamSiteUrl")] string TeamSiteUrl,
			[property: JsonProperty("teamSiteDomain")] string TeamSiteDomain,
			[property: JsonProperty("teamSitePath")] string TeamSitePath,
			[property: JsonProperty("teamSiteId")] string TeamSiteId,
			[property: JsonProperty("mySitePath")] string MySitePath,
			[property: JsonProperty("mySiteDomain")] string MySiteDomain
		);

		public record Team(
			[property: JsonProperty("internalId")] string InternalId,
			[property: JsonProperty("displayName")] string DisplayName,
			[property: JsonProperty("type")] int? Type,
			[property: JsonProperty("groupId")] string GroupId,
			[property: JsonProperty("templateId")] string TemplateId,
			[property: JsonProperty("isArchived")] bool? IsArchived,
			[property: JsonProperty("userRole")] int? UserRole
		);

		public record Tenant(
			[property: JsonProperty("id")] string Id,
			[property: JsonProperty("teamsSku")] string TeamsSku
		);

		public record User(
			[property: JsonProperty("id")] string Id,
			[property: JsonProperty("licenseType")] string LicenseType,
			[property: JsonProperty("loginHint")] string LoginHint,
			[property: JsonProperty("userPrincipalName")] string UserPrincipalName,
			[property: JsonProperty("tenant")] Tenant Tenant
		);
	}
}
