using SixLabors.ImageSharp.PixelFormats;

namespace SinaMN75U.Routes;

public static class DataSeedRoutes {
	public static void MapDataSeedRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Users", async (IDataSeedService s) => (await s.SeedUsers()).ToResult());
		r.MapPost("Categories", async (IDataSeedService s) => (await s.SeedCategories()).ToResult());
		r.MapPost("Contents", async (IDataSeedService s) => (await s.SeedContents()).ToResult());
		r.MapPost("HotelsAndDorms", async (IDataSeedService s, CancellationToken c) => (await s.SeedHotelsAndDorms(c)).ToResult()).Produces<UResponse<List<KeyValue>>>();
		r.MapPost("Parking", async (ParkingSeedParams d, IParkingSeedService s, CancellationToken c) => (await s.SeedParking(d, c)).ToResult()).Produces<UResponse<ParkingSeedResponse>>();
		r.MapPost("Sportopia", async (SportSeedParams d, ISportSeedService s, CancellationToken c) => (await s.SeedSportopia(d, c)).ToResult()).Produces<UResponse<SportSeedResponse>>();
	}
}

public sealed class SportSeedParams : BaseParams {
	/// Removes everything a previous run of this seed created (its users are kept and refreshed) before writing it again.
	public bool Reset { get; set; } = true;
}


public sealed class SportSeedAccountResponse {
	public required string Email { get; set; }
	public required string Password { get; set; }
	public required string FullName { get; set; }
	public required string Role { get; set; }
}

public sealed class SportSeedResponse {
	public ICollection<SportSeedAccountResponse> Accounts { get; set; } = [];
	public int Users { get; set; }
	public int Venues { get; set; }
	public int Courts { get; set; }
	public int Bookings { get; set; }
	public int Tournaments { get; set; }
	public int TournamentMatches { get; set; }
	public int OpenMatches { get; set; }
	public int Posts { get; set; }
	public int Conversations { get; set; }
	public int Messages { get; set; }
	public int Notifications { get; set; }

	/// Steps a service refused; the rest of the seed still ran.
	public ICollection<string> Warnings { get; set; } = [];
}


public interface ISportSeedService {
	Task<UResponse<SportSeedResponse?>> SeedSportopia(SportSeedParams p, CancellationToken ct);
}

/// One-call demo data for Sportopia: about thirty players (levels, followers, wallets, photos), clubs with courts,
/// photos and reviews, finished / running / open tournaments, open games, bookings, posts, stories, chats and the
/// notifications they cause. The sport, venue and booking actions go through the real services signed in as the seed
/// users, so levels, trophies, badges, wallet payments and notifications come out exactly as the app makes them; each
/// past action is then moved back to its date. Every seed user has a fixed id, so Reset removes what an earlier run made
/// and nothing else. Sign in to the app with demo@sportopia.app / Sport@1234.
public class SportSeedService(
	DbContext db,
	ITokenService ts,
	ISportService sports,
	IVenueService venues,
	IWebHostEnvironment env
) : ISportSeedService {
	private const string SeedNamespace = "sportopia-seed";
	private const string DefaultPassword = "Sport@1234";
	private const string EmailDomain = "sportopia.app";

	private sealed record SeedUser(string Key, string First, string Last, bool Female, string Country, string City, string Bio, string Role, decimal Balance, (TagSport Sport, decimal Level)[] Sports, bool Photo = true);

	private sealed record SeedCourt(string Key, string Title, TagSport Sport, bool Indoor, decimal Price, int Slot, string Surface, int Players, List<CourtPriceRule>? Rules = null);

	private sealed record SeedVenue(
		string Key, string Owner, string Title, TagVenue Kind, TagVenue Status, double Lat, double Lng, string City, string Address, string Phone, string Instagram,
		string Description, string[] Amenities, string Open, string Close, bool PayAtVenue, int FreeHours, int Penalty, string[] Staff, SeedCourt[] Courts,
		(byte R, byte G, byte B) Color, string? RejectionReason = null);

	private static readonly SeedUser[] Users = [
		new("demo", "آرمان", "کیانی", false, "IR", "Tehran", "عاشق پدل، آخر هفته‌ها تنیس. صاحب باغ پدل کیانی 🎾", "Demo: player + club owner + organizer", 20_000_000,
			[(TagSport.Padel, 4.2m), (TagSport.Tennis, 3.6m), (TagSport.Squash, 3.0m), (TagSport.Billiards, 3.5m), (TagSport.Snooker, 2.8m)]),
		new("owner", "مهرداد", "علوی", false, "IR", "Tehran", "مؤسس پدل آرنا تهران و باشگاه راکتی انقلاب", "Club owner", 2_000_000, [(TagSport.Padel, 3.8m), (TagSport.Tennis, 4.0m)]),
		new("organizer", "کیمیا", "صادقی", true, "IR", "Tehran", "مدیر برگزاری مسابقات پدل و اسکواش", "Organizer + staff of Padel Arena", 5_000_000, [(TagSport.Padel, 3.2m), (TagSport.Squash, 3.5m)]),
		new("neda", "ندا", "شریفی", true, "IR", "Tehran", "پدل هر روز صبح ☀️", "Player", 12_000_000, [(TagSport.Padel, 4.6m), (TagSport.Tennis, 4.1m)]),
		new("kaveh", "کاوه", "مرادی", false, "IR", "Tehran", "دست راست، بک‌هند قوی", "Player", 12_000_000, [(TagSport.Padel, 5.1m), (TagSport.Squash, 4.4m)]),
		new("sara", "سارا", "احمدی", true, "IR", "Tehran", "تنیس‌باز سابق، تازه پدل رو شروع کردم", "Player", 12_000_000, [(TagSport.Tennis, 4.8m), (TagSport.Padel, 3.9m)]),
		new("reza", "رضا", "توکلی", false, "IR", "Tehran", "فوتسال، پدل، بیلیارد — هرچی توپ داره", "Player", 12_000_000, [(TagSport.Padel, 4.4m), (TagSport.Football, 4.5m), (TagSport.Billiards, 4.0m)]),
		new("mina", "مینا", "رحیمی", true, "IR", "Isfahan", "صاحب مرکز راکتی اصفهان", "Club owner (Isfahan)", 12_000_000, [(TagSport.Padel, 3.3m), (TagSport.Tennis, 3.8m)]),
		new("babak", "بابک", "جعفری", false, "IR", "Tehran", "کاپیتان شیرهای آزادی", "Player", 12_000_000, [(TagSport.Football, 5.0m), (TagSport.Padel, 4.0m), (TagSport.Snooker, 4.2m)]),
		new("leila", "لیلا", "کریمی", true, "IR", "Shiraz", "شیراز، زمین فوتبال پنج نفره و اسکواش", "Club owner (Shiraz)", 12_000_000, [(TagSport.Squash, 3.9m), (TagSport.Padel, 3.6m)]),
		new("omid", "امید", "حسینی", false, "IR", "Tehran", "", "Player", 12_000_000, [(TagSport.Padel, 4.8m), (TagSport.Tennis, 4.4m), (TagSport.Football, 4.0m)], false),
		new("shirin", "شیرین", "نجفی", true, "IR", "Tehran", "تازه‌کار ولی پرانرژی", "Player (invited by demo)", 12_000_000, [(TagSport.Padel, 2.9m), (TagSport.Tennis, 3.1m)]),
		new("pouya", "پویا", "ابراهیمی", false, "IR", "Mashhad", "بیلیارد و اسنوکر، مشهد", "Player (pending venue)", 12_000_000, [(TagSport.Billiards, 5.2m), (TagSport.Snooker, 4.9m), (TagSport.Padel, 3.7m)]),
		new("yasaman", "یاسمن", "قاسمی", true, "IR", "Tehran", "اسکواش صبح، پدل عصر", "Player", 12_000_000, [(TagSport.Padel, 4.3m), (TagSport.Squash, 4.1m)]),
		new("farhad", "فرهاد", "زند", false, "IR", "Tehran", "مربی پدل · هم‌تیمی آرمان", "Player", 12_000_000, [(TagSport.Padel, 5.4m), (TagSport.Tennis, 5.0m)]),
		new("niloufar", "نیلوفر", "باقری", true, "IR", "Kish", "ساحل کیش، پدل کنار دریا 🌊", "Club owner (Kish)", 12_000_000, [(TagSport.Padel, 3.5m), (TagSport.Tennis, 3.4m)]),
		new("amir", "امیر", "صالحی", false, "IR", "Tehran", "", "Player", 12_000_000, [(TagSport.Padel, 4.1m), (TagSport.Football, 4.2m), (TagSport.Squash, 3.7m)], false),
		new("parisa", "پریسا", "موسوی", true, "IR", "Tehran", "بیلیارد رو جدی گرفتم", "Player", 12_000_000, [(TagSport.Padel, 3.8m), (TagSport.Billiards, 3.6m)]),
		new("hamed", "حامد", "رستمی", false, "IR", "Tabriz", "تبریز · فوتبال و اسنوکر", "Player", 12_000_000, [(TagSport.Football, 4.8m), (TagSport.Padel, 3.4m), (TagSport.Snooker, 3.8m)]),
		new("tara", "تارا", "عزیزی", true, "IR", "Tehran", "تنیس و پدل، چهارشنبه‌ها پایه‌ام", "Player", 12_000_000, [(TagSport.Padel, 4.5m), (TagSport.Tennis, 4.6m)]),
		new("siavash", "سیاوش", "کمالی", false, "IR", "Tehran", "استخر و میز اسنوکر", "Player (rejected venue)", 12_000_000, [(TagSport.Billiards, 4.6m), (TagSport.Snooker, 4.4m), (TagSport.Padel, 3.1m)]),
		new("ghazal", "غزل", "فراهانی", true, "IR", "Isfahan", "", "Player", 12_000_000, [(TagSport.Squash, 3.2m), (TagSport.Padel, 3.0m)], false),
		new("ali", "علی", "دانشور", false, "IR", "Tehran", "کاپیتان ستاره‌های شمال", "Player", 12_000_000, [(TagSport.Padel, 4.7m), (TagSport.Football, 4.6m), (TagSport.Tennis, 3.9m)]),
		new("mahsa", "مهسا", "نیک‌پور", true, "IR", "Tehran", "دعوت‌شده توسط آرمان 🙌", "Player (invited by demo)", 12_000_000, [(TagSport.Padel, 3.6m), (TagSport.Tennis, 3.3m), (TagSport.Billiards, 3.0m)]),
		new("lucas", "Lucas", "Fernández", false, "ES", "Barcelona", "Padel coach from Barcelona. Here for the winter season.", "Player (Spain)", 12_000_000, [(TagSport.Padel, 5.6m), (TagSport.Tennis, 4.5m)]),
		new("emma", "Emma", "Schulz", true, "DE", "Berlin", "Tennis first, padel second.", "Player (Germany)", 12_000_000, [(TagSport.Tennis, 5.2m), (TagSport.Padel, 4.0m), (TagSport.Squash, 4.0m)]),
		new("omar", "Omar", "Haddad", false, "AE", "Dubai", "Dubai Falcons captain · squash & padel", "Player (UAE)", 12_000_000, [(TagSport.Padel, 5.0m), (TagSport.Squash, 4.6m), (TagSport.Football, 4.4m)]),
		new("elif", "Elif", "Yılmaz", true, "TR", "Istanbul", "Istanbul ↔ Tehran", "Player (Turkey)", 12_000_000, [(TagSport.Padel, 3.9m), (TagSport.Tennis, 4.2m)]),
		new("james", "James", "Carter", false, "GB", "London", "Squash and snooker. Tea after every match.", "Player (UK)", 12_000_000, [(TagSport.Squash, 5.3m), (TagSport.Tennis, 4.3m), (TagSport.Snooker, 5.0m), (TagSport.Billiards, 4.4m)]),
		new("sofia", "Sofia", "Rossi", true, "IT", "Milan", "", "Player (Italy)", 12_000_000, [(TagSport.Padel, 4.4m), (TagSport.Tennis, 4.0m)], false),
		new("behrooz", "بهروز", "نصیری", false, "IR", "Tehran", "فروش راکت ارزان، دایرکت بدید!!!", "Spammer: blocked and reported", 500_000, [(TagSport.Padel, 2.5m)], false)
	];

	// Who registered with whose referral code (three or more earn the recruiter badge).
	private static readonly Dictionary<string, string> ReferredBy = new() {
		["shirin"] = "demo", ["mahsa"] = "demo", ["niloufar"] = "demo", ["behrooz"] = "demo", ["parisa"] = "neda", ["ghazal"] = "mina"
	};

	private static readonly string[] AllAmenities = ["parking", "shower", "locker", "cafe", "light", "indoor", "rent"];

	private static List<CourtPriceRule> PeakRules(decimal evening, decimal weekend) => [
		new() { From = "17:00", To = "24:00", PricePerHour = evening },
		new() { Days = [4, 5], From = "00:00", To = "24:00", PricePerHour = weekend }
	];

	private static readonly SeedVenue[] Venues = [
		new("arena", "owner", "پدل آرنا تهران", TagVenue.Club, TagVenue.Approved, 35.7915, 51.4105, "Tehran", "تهران، ولیعصر، بالاتر از پارک‌وی، کوچه سپیده، پلاک ۸", "02122001100", "padelarena.tehran",
			"بزرگ‌ترین مجموعه پدل تهران با دو زمین سرپوشیده‌ی پانوراما و دو زمین روباز. اجاره‌ی راکت، کلاس‌های گروهی و تورنمنت‌های ماهانه.",
			AllAmenities, "07:00", "24:00", true, 24, 50, ["organizer"], [
				new("arena-1", "زمین ۱ · پانوراما", TagSport.Padel, true, 900_000, 90, "چمن مصنوعی آبی", 4, PeakRules(1_200_000, 1_300_000)),
				new("arena-2", "زمین ۲ · پانوراما", TagSport.Padel, true, 900_000, 90, "چمن مصنوعی آبی", 4, PeakRules(1_200_000, 1_300_000)),
				new("arena-3", "زمین ۳ · روباز", TagSport.Padel, false, 700_000, 90, "چمن مصنوعی سبز", 4, PeakRules(900_000, 1_000_000)),
				new("arena-4", "زمین ۴ · روباز", TagSport.Padel, false, 700_000, 90, "چمن مصنوعی سبز", 4)
			], (30, 90, 200)),
		new("enghelab", "owner", "باشگاه راکتی انقلاب", TagVenue.Club, TagVenue.Approved, 35.7012, 51.3952, "Tehran", "تهران، خیابان انقلاب، مجموعه ورزشی انقلاب، درب شمالی", "02166001200", "enghelab.racket",
			"زمین‌های تنیس خاک رس و هارد، به‌علاوه‌ی دو سالن اسکواش استاندارد. پرداخت در محل هم ممکنه.",
			["parking", "shower", "locker", "cafe", "light"], "06:00", "23:00", true, 12, 100, [], [
				new("enghelab-t1", "تنیس ۱ · خاک رس", TagSport.Tennis, false, 600_000, 60, "خاک رس", 4, [new() { From = "16:00", To = "23:00", PricePerHour = 750_000 }]),
				new("enghelab-t2", "تنیس ۲ · هارد", TagSport.Tennis, false, 550_000, 60, "هارد کورت", 4),
				new("enghelab-t3", "تنیس ۳ · سرپوشیده", TagSport.Tennis, true, 800_000, 60, "هارد کورت", 4),
				new("enghelab-s1", "اسکواش ۱", TagSport.Squash, true, 400_000, 60, "پارکت", 2),
				new("enghelab-s2", "اسکواش ۲", TagSport.Squash, true, 400_000, 60, "پارکت", 2)
			], (200, 90, 40)),
		new("cue", "owner", "کیو کلاب ونک", TagVenue.Club, TagVenue.Approved, 35.7575, 51.4097, "Tehran", "تهران، میدان ونک، برج نگار، طبقه منفی یک", "02188001300", "cueclub.vanak",
			"میزهای بیلیارد آمریکایی و اسنوکر استاندارد، با کافه و فضای آرام.",
			["parking", "cafe", "indoor", "rent"], "12:00", "24:00", false, 48, 50, [], [
				new("cue-b1", "میز بیلیارد ۱", TagSport.Billiards, true, 250_000, 60, "ماهوت سبز", 2),
				new("cue-b2", "میز بیلیارد ۲", TagSport.Billiards, true, 250_000, 60, "ماهوت سبز", 2),
				new("cue-b3", "میز بیلیارد ۳", TagSport.Billiards, true, 250_000, 60, "ماهوت آبی", 2),
				new("cue-s1", "میز اسنوکر ۱", TagSport.Snooker, true, 350_000, 60, "ماهوت سبز", 2),
				new("cue-s2", "میز اسنوکر ۲", TagSport.Snooker, true, 350_000, 60, "ماهوت سبز", 2)
			], (20, 110, 60)),
		new("garden", "demo", "باغ پدل کیانی", TagVenue.Club, TagVenue.Approved, 35.7570, 51.3700, "Tehran", "تهران، شهرک غرب، بلوار دریا، باغ کیانی", "02188002400", "kiani.padel",
			"دو زمین پدل وسط باغ؛ یکی سرپوشیده و یکی روباز زیر درخت‌ها. پارکینگ رایگان.",
			["parking", "shower", "light", "cafe"], "08:00", "23:00", true, 24, 100, [], [
				new("garden-1", "زمین سرپوشیده", TagSport.Padel, true, 850_000, 90, "چمن مصنوعی آبی", 4, PeakRules(1_050_000, 1_100_000)),
				new("garden-2", "زمین روباز", TagSport.Padel, false, 650_000, 90, "چمن مصنوعی سبز", 4)
			], (40, 140, 90)),
		new("kish", "niloufar", "پدل ساحلی کیش", TagVenue.Club, TagVenue.Approved, 26.5337, 53.9805, "Kish", "کیش، بلوار ساحل، روبروی اسکله تفریحی", "07644001500", "kish.beach.padel",
			"پدل کنار دریا با غروب‌های دیدنی. زمین‌ها روباز و با نورپردازی شبانه.",
			["parking", "shower", "light", "rent"], "08:00", "23:00", false, 24, 100, [], [
				new("kish-1", "زمین ساحلی ۱", TagSport.Padel, false, 650_000, 90, "چمن مصنوعی", 4),
				new("kish-2", "زمین ساحلی ۲", TagSport.Padel, false, 650_000, 90, "چمن مصنوعی", 4)
			], (20, 150, 190)),
		new("isfahan", "mina", "مرکز راکتی اصفهان", TagVenue.Club, TagVenue.Approved, 32.6539, 51.6660, "Isfahan", "اصفهان، خیابان چهارباغ بالا، کوچه ۱۲", "03136001600", "isfahan.racket",
			"پدل و تنیس در قلب اصفهان.",
			["parking", "shower", "locker"], "07:00", "22:00", true, 24, 100, [], [
				new("isfahan-p1", "پدل ۱", TagSport.Padel, true, 600_000, 90, "چمن مصنوعی", 4),
				new("isfahan-p2", "پدل ۲", TagSport.Padel, false, 500_000, 90, "چمن مصنوعی", 4),
				new("isfahan-t1", "تنیس", TagSport.Tennis, false, 450_000, 60, "خاک رس", 4)
			], (180, 120, 50)),
		new("shiraz", "leila", "فوتبال پنج‌نفره شیراز", TagVenue.Club, TagVenue.Approved, 29.6100, 52.5310, "Shiraz", "شیراز، بلوار چمران، جنب پارک آزادی", "07136001700", "shiraz.fivesside",
			"دو زمین چمن مصنوعی استاندارد پنج‌نفره با رختکن و دوش.",
			["parking", "shower", "locker", "light"], "08:00", "24:00", true, 24, 100, [], [
				new("shiraz-1", "زمین A", TagSport.Football, false, 1_500_000, 60, "چمن مصنوعی نسل ۴", 10),
				new("shiraz-2", "زمین B", TagSport.Football, false, 1_300_000, 60, "چمن مصنوعی نسل ۳", 10)
			], (60, 130, 50)),
		new("shop", "owner", "فروشگاه راکت تهران", TagVenue.Shop, TagVenue.Approved, 35.7600, 51.4150, "Tehran", "تهران، خیابان گاندی، پلاک ۲۲", "02188001800", "racketshop.tehran",
			"راکت پدل و تنیس، توپ، کفش و زه‌کشی حرفه‌ای. نمایندگی برندهای معتبر.",
			["parking"], "10:00", "21:00", false, 24, 100, [], [], (120, 60, 160)),
		new("mashhad", "pouya", "پدل هاب مشهد", TagVenue.Club, TagVenue.Pending, 36.2970, 59.6060, "Mashhad", "مشهد، بلوار وکیل‌آباد، نبش وکیل‌آباد ۲۰", "05138001900", "padelhub.mashhad",
			"مجموعه‌ی تازه‌ی پدل در مشهد؛ منتظر تأیید ادمین.",
			["parking", "shower"], "08:00", "23:00", false, 24, 100, [], [
				new("mashhad-1", "زمین ۱", TagSport.Padel, true, 550_000, 90, "چمن مصنوعی", 4)
			], (150, 70, 70)),
		new("lavasan", "siavash", "دهکده ورزشی لواسان", TagVenue.Club, TagVenue.Rejected, 35.8240, 51.6330, "Tehran", "لواسان", "", "",
			"", [], "08:00", "22:00", false, 24, 100, [], [], (90, 90, 90), "عکس‌ها و نشانی دقیق مجموعه کامل نیست. لطفاً اطلاعات را تکمیل کنید.")
	];

	private readonly Dictionary<string, UserEntity> _users = new();
	private readonly Dictionary<string, string> _tokens = new();
	private readonly Dictionary<Guid, string> _keyOf = new();
	private readonly Dictionary<TagSport, SportEntity> _sports = new();
	private readonly Dictionary<string, (SeedVenue Venue, SeedCourt Court)> _courts = new();
	private readonly HashSet<(string Court, int Day, int Slot)> _usedSlots = [];
	private readonly Random _random = new(20261003);
	private readonly SportSeedResponse _result = new();
	private List<Guid> _userIds = [];
	private string _adminToken = "";
	private int _tempDay = 150;
	private CancellationToken _ct;

	private static Guid SeedId(string key) => new(MD5.HashData(Encoding.UTF8.GetBytes($"{SeedNamespace}:{key}")));

	private static string Email(string key) => $"{key}@{EmailDomain}";

	private static readonly TimeZoneInfo Tehran = FindZone("Asia/Tehran");

	private static TimeZoneInfo FindZone(string id) {
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(id);
		}
		catch {
			return TimeZoneInfo.Utc;
		}
	}

	private Guid U(string key) => _users[key].Id;
	private string T(string key) => _tokens[key];
	private string Name(string key) => $"{_users[key].FirstName} {_users[key].LastName}";

	private decimal LevelOf(string key, TagSport sport) => Users.First(x => x.Key == key).Sports.FirstOrDefault(x => x.Sport == sport).Level;

	private List<string> Roster(TagSport sport) => Users.Where(x => x.Key != "behrooz" && x.Sports.Any(s => s.Sport == sport)).Select(x => x.Key).ToList();

	public async Task<UResponse<SportSeedResponse?>> SeedSportopia(SportSeedParams p, CancellationToken ct) {
		_ct = ct;
		_userIds = Users.Select(x => SeedId($"user:{x.Key}")).ToList();
		DateTime now = DateTime.UtcNow;

		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == Core.App.Users.SystemAdmin.Id, ct))
			return new UResponse<SportSeedResponse?>(null, Usc.NotFound, "The system admin user doesn't exist yet; call DataSeeder/Users first.");
		if (p.Reset) await Wipe();
		else if (await db.Set<VenueEntity>().AnyAsync(x => x.Id == SeedId("venue:arena"), ct))
			return new UResponse<SportSeedResponse?>(null, Usc.Conflict, "Already seeded; call again with Reset = true.");

		string? conflict = await FindConflict();
		if (conflict != null) return new UResponse<SportSeedResponse?>(null, Usc.Conflict, conflict);

		await SeedSports(now);
		await SeedAccounts(now);
		await SeedProfiles(now);
		await SeedFollows(now);
		await SeedVenues(now);
		await SeedHistory(now);
		await SeedUpcoming(now);
		await SeedBookings(now);
		await SeedPosts(now);
		await SeedChats(now);
		await FinishNotifications(now);
		await Count();

		ULog.Success($"Sportopia seed: {_result.Users} users, {_result.Venues} venues, {_result.Tournaments} tournaments, {_result.OpenMatches} games, {_result.Bookings} bookings, {_result.Warnings.Count} warnings.");
		return new UResponse<SportSeedResponse?>(_result, Usc.Created);
	}

	// ---------------- Helpers ----------------

	/// <summary>Checks a service answer; a refusal is kept as a warning and the seed goes on.</summary>
	private bool Ok(UResponse r, string step) {
		db.ChangeTracker.Clear();
		if ((int)r.Status < 300) return true;
		_result.Warnings.Add($"{step}: {r.Message} ({r.Status})");
		ULog.Warning($"Sportopia seed: {step}: {r.Message} ({r.Status})");
		return false;
	}

	/// <summary>Runs [action] now and then moves everything it created for the seed users back (or forward) to [when].</summary>
	private async Task At(DateTime when, Func<Task> action) {
		DateTime before = DateTime.UtcNow;
		await action();
		db.ChangeTracker.Clear();
		TimeSpan shift = when - before;
		List<Guid> ids = _userIds;
		CancellationToken ct = _ct;
		await db.Set<NotificationEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.UserId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<WalletTxnEntity>().Where(x => x.CreatedAt >= before && (ids.Contains(x.SenderId) || ids.Contains(x.ReceiverId))).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<PlayerAchievementEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.UserId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<PlayerRatingHistoryEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.UserId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<TournamentEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<TournamentEntryEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<TournamentMatchEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<OpenMatchEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<BookingEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<VenueEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<CourtEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
	}

	private T Pick<T>(IReadOnlyList<T> list) => list[_random.Next(list.Count)];

	private List<T> Shuffle<T>(IEnumerable<T> items) => items.OrderBy(_ => _random.Next()).ToList();

	/// <summary>The level on the sport's own scale (the seed data uses 1-7).</summary>
	private decimal Scale(TagSport sport, decimal level) {
		SportEntity s = _sports[sport];
		decimal scaled = s.MinLevel + (level - 1) / 6 * (s.MaxLevel - s.MinLevel);
		return Math.Round(Math.Clamp(scaled, s.MinLevel, s.MaxLevel), 2);
	}

	private static TimeSpan ParseTime(string value) => value == "24:00" ? TimeSpan.FromHours(24) : TimeSpan.Parse(value, CultureInfo.InvariantCulture);

	/// <summary>The UTC start of the [slot]th slot of a court on a day (days from today, venue time).</summary>
	private static DateTime SlotUtc(SeedVenue v, SeedCourt c, int day, int slot) {
		DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tehran).Date;
		DateTime local = today.AddDays(day) + ParseTime(v.Open) + TimeSpan.FromMinutes(slot * c.Slot);
		return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Tehran);
	}

	private static int SlotsPerDay(SeedVenue v, SeedCourt c) => (int)((ParseTime(v.Close) - ParseTime(v.Open)).TotalMinutes / c.Slot);

	/// <summary>Today in Tehran at [hour], plus [days].</summary>
	private static DateTime LocalAt(int days, double hour) {
		DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tehran).Date;
		return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(today.AddDays(days).AddHours(hour), DateTimeKind.Unspecified), Tehran);
	}

	// ---------------- Reset ----------------

	/// <summary>
	/// Removes what an earlier run made. The seed users stay (other tables, like ApiLogs, may point at them) and are refreshed;
	/// what the platform still holds for them is taken back out of the system admin's wallet so its balance stays right.
	/// </summary>
	private async Task Wipe() {
		List<Guid> ids = _userIds;
		Guid admin = Core.App.Users.SystemAdmin.Id;
		CancellationToken ct = _ct;

		decimal held = await db.Set<WalletTxnEntity>().Where(x => ids.Contains(x.SenderId) && x.ReceiverId == admin && !x.Tags.Contains(TagWalletTxn.Charge)).SumAsync(x => x.Amount, ct)
		               - await db.Set<WalletTxnEntity>().Where(x => x.SenderId == admin && ids.Contains(x.ReceiverId) && !x.Tags.Contains(TagWalletTxn.Charge)).SumAsync(x => x.Amount, ct);
		if (held != 0) await db.Set<WalletEntity>().Where(x => x.CreatorId == admin).ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance - held), ct);
		await db.Set<WalletTxnEntity>().Where(x => ids.Contains(x.SenderId) || ids.Contains(x.ReceiverId)).ExecuteDeleteAsync(ct);

		await db.Set<NotificationEntity>().Where(x => ids.Contains(x.UserId) || ids.Contains(x.CreatorId)).ExecuteDeleteAsync(ct);

		List<Guid> conversations = await db.Set<ConversationEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		await db.Set<MessageEntity>().Where(x => conversations.Contains(x.ConversationId) || ids.Contains(x.CreatorId)).ExecuteDeleteAsync(ct);
		await db.Set<ConversationEntity>().Where(x => conversations.Contains(x.Id)).ExecuteDeleteAsync(ct);

		await db.Set<BlockEntity>().Where(x => ids.Contains(x.CreatorId) || ids.Contains(x.BlockedUserId)).ExecuteDeleteAsync(ct);
		List<Guid> posts = await db.Set<PostEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		// Replies of other users to the seed posts go with them, deepest first.
		List<Guid> level = posts;
		for (int depth = 0; depth < 6 && level.Count > 0; depth++) {
			List<Guid> parents = level;
			level = await db.Set<PostEntity>().Where(x => x.ParentId != null && parents.Contains(x.ParentId.Value) && !posts.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
			posts.AddRange(level);
		}

		await db.Set<ReportEntity>().Where(x => ids.Contains(x.CreatorId) || ids.Contains(x.TargetId) || posts.Contains(x.TargetId)).ExecuteDeleteAsync(ct);
		await db.Set<MediaEntity>().Where(x => x.PostId != null && posts.Contains(x.PostId.Value)).ExecuteDeleteAsync(ct);
		for (int depth = 0; depth < 8; depth++) {
			int deleted = await db.Set<PostEntity>().Where(x => posts.Contains(x.Id) && !x.Children.Any()).ExecuteDeleteAsync(ct);
			if (deleted == 0) break;
		}

		await db.Set<FollowEntity>().Where(x => ids.Contains(x.CreatorId) || x.UserId != null && ids.Contains(x.UserId.Value)).ExecuteDeleteAsync(ct);

		List<Guid> venueIds = await db.Set<VenueEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		await db.Set<BookingEntity>().Where(x => venueIds.Contains(x.VenueId) || ids.Contains(x.UserId)).ExecuteDeleteAsync(ct);
		await db.Set<CommentEntity>().Where(x => ids.Contains(x.CreatorId) || x.VenueId != null && venueIds.Contains(x.VenueId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<MediaEntity>().Where(x => x.VenueId != null && venueIds.Contains(x.VenueId.Value) || x.UserId != null && ids.Contains(x.UserId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<OpenMatchEntity>().Where(x => x.VenueId != null && venueIds.Contains(x.VenueId.Value) && !ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.VenueId, (Guid?)null), ct);
		await db.Set<OpenMatchEntity>().Where(x => ids.Contains(x.CreatorId)).ExecuteDeleteAsync(ct);

		List<Guid> tournaments = await db.Set<TournamentEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		await db.Set<PlayerAchievementEntity>().Where(x => ids.Contains(x.UserId) || x.TournamentId != null && tournaments.Contains(x.TournamentId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<PlayerRatingHistoryEntity>().Where(x => ids.Contains(x.UserId)).ExecuteDeleteAsync(ct);
		await db.Set<TournamentMatchEntity>().Where(x => tournaments.Contains(x.TournamentId)).ExecuteDeleteAsync(ct);
		await db.Set<TournamentEntryEntity>().Where(x => tournaments.Contains(x.TournamentId)).ExecuteDeleteAsync(ct);
		await db.Set<TournamentEntity>().Where(x => tournaments.Contains(x.Id)).ExecuteDeleteAsync(ct);

		await db.Set<CourtEntity>().Where(x => venueIds.Contains(x.VenueId)).ExecuteDeleteAsync(ct);
		await db.Set<VenueEntity>().Where(x => venueIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
		await db.Set<PlayerSportProfileEntity>().Where(x => ids.Contains(x.UserId)).ExecuteDeleteAsync(ct);
	}

	/// <summary>A seed email or user name that already belongs to another account would make the insert fail.</summary>
	private async Task<string?> FindConflict() {
		List<string> emails = Users.Select(x => Email(x.Key)).ToList();
		List<string> userNames = Users.Select(x => $"sp.{x.Key}").ToList();
		List<Guid> ids = _userIds;
		var taken = await db.Set<UserEntity>()
			.Where(x => !ids.Contains(x.Id) && (x.Email != null && emails.Contains(x.Email) || userNames.Contains(x.UserName)))
			.Select(x => new { x.Id, x.Email, x.UserName })
			.FirstOrDefaultAsync(_ct);
		return taken == null ? null : $"User {taken.Id} already uses {taken.Email ?? taken.UserName}; remove it or change the seed's email domain.";
	}

	// ---------------- Catalog, accounts, profiles, follows ----------------

	private async Task SeedSports(DateTime now) {
		(TagSport Type, string Title)[] catalog = [
			(TagSport.Padel, ULocalizedConstants.Padel), (TagSport.Tennis, ULocalizedConstants.Tennis), (TagSport.Squash, ULocalizedConstants.Squash),
			(TagSport.Billiards, ULocalizedConstants.Billiards), (TagSport.Snooker, ULocalizedConstants.Snooker), (TagSport.Football, ULocalizedConstants.Football),
			(TagSport.Karate, ULocalizedConstants.Karate)
		];
		for (int i = 0; i < catalog.Length; i++) {
			(TagSport type, string title) = catalog[i];
			SportEntity? s = await db.Set<SportEntity>().AsTracking().FirstOrDefaultAsync(x => x.Tags.Contains(type), _ct);
			if (s == null) {
				// Karate stays "coming soon" so that state shows up too.
				s = new SportEntity {
					Id = SeedId($"sport:{type}"),
					CreatedAt = now.AddDays(-90),
					CreatorId = Core.App.Users.SystemAdmin.Id,
					Tags = [type, type == TagSport.Karate ? TagSport.ComingSoon : TagSport.Active],
					Title = title,
					Order = i + 1,
					JsonData = new SportJson()
				};
				await db.Set<SportEntity>().AddAsync(s, _ct);
			}
			// The seed plays every sport but karate, so they have to be open.
			else if (type != TagSport.Karate && !s.Tags.Contains(TagSport.Active)) s.Tags = s.Tags.Where(x => (int)x < 200).Append(TagSport.Active).ToList();

			_sports[type] = s;
		}

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	private async Task SeedAccounts(DateTime now) {
		Guid admin = Core.App.Users.SystemAdmin.Id;
		for (int i = 0; i < Users.Length; i++) {
			SeedUser s = Users[i];
			Guid id = SeedId($"user:{s.Key}");
			UserJson json = new() {
				Country = s.Country,
				City = s.City,
				ReferralCode = $"{s.Key.ToUpperInvariant()}{(i * 37 + 11) % 90 + 10}",
				ReferrerId = ReferredBy.TryGetValue(s.Key, out string? referrer) ? SeedId($"user:{referrer}") : null
			};
			UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == id, _ct);
			if (e == null) {
				e = new UserEntity {
					Id = id,
					CreatedAt = now.AddDays(-75 + i),
					CreatorId = id,
					JsonData = json,
					Tags = [s.Female ? TagUser.Female : TagUser.Male, TagUser.Verified],
					UserName = $"sp.{s.Key}",
					Password = UPasswordHasher.Hash(DefaultPassword),
					RefreshToken = ts.GenerateRefreshToken(),
					Email = Email(s.Key)
				};
				await db.Set<UserEntity>().AddAsync(e, _ct);
			}
			else {
				e.JsonData.Country = json.Country;
				e.JsonData.City = json.City;
				e.JsonData.ReferralCode = json.ReferralCode;
				e.JsonData.ReferrerId = json.ReferrerId;
				e.Tags = [s.Female ? TagUser.Female : TagUser.Male, TagUser.Verified];
				e.UserName = $"sp.{s.Key}";
				e.Password = UPasswordHasher.Hash(DefaultPassword);
				e.Email = Email(s.Key);
			}

			e.FirstName = s.First;
			e.LastName = s.Last;
			e.Bio = s.Bio.IsNullOrEmpty() ? null : s.Bio;
			e.Birthdate = new DateTime(1985 + i % 17, i % 12 + 1, i % 27 + 1, 0, 0, 0, DateTimeKind.Utc);
			_users[s.Key] = e;
			_keyOf[id] = s.Key;
			_result.Accounts.Add(new SportSeedAccountResponse { Email = Email(s.Key), Password = DefaultPassword, FullName = $"{s.First} {s.Last}", Role = s.Role });
		}

		await db.SaveChangesAsync(_ct);

		// Wallets: the seed users get their starting balance (shown as a top-up), and the platform's escrow wallet must exist.
		foreach (SeedUser s in Users) {
			Guid id = U(s.Key);
			WalletEntity? w = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == id, _ct);
			if (w == null) await db.Set<WalletEntity>().AddAsync(new WalletEntity { Id = SeedId($"wallet:{s.Key}"), CreatorId = id, CreatedAt = now.AddDays(-60), JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = s.Balance }, _ct);
			else w.Balance = s.Balance;
			await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
				Id = SeedId($"charge:{s.Key}"),
				CreatedAt = now.AddDays(-45),
				CreatorId = id,
				SenderId = admin,
				ReceiverId = id,
				Amount = s.Balance,
				Tags = [TagWalletTxn.Charge],
				JsonData = new WalletTxnJson()
			}, _ct);
		}

		if (!await db.Set<WalletEntity>().AnyAsync(x => x.CreatorId == admin, _ct))
			await db.Set<WalletEntity>().AddAsync(new WalletEntity { Id = Guid.CreateVersion7(), CreatorId = admin, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }, _ct);

		// Profile photos: drawn here, so the seed needs no files of its own.
		for (int i = 0; i < Users.Length; i++) {
			SeedUser s = Users[i];
			if (!s.Photo) continue;
			string path = $"users/seed-{s.Key}.jpg";
			DrawAvatar(path, i);
			await db.Set<MediaEntity>().AddAsync(new MediaEntity { Id = SeedId($"avatar:{s.Key}"), CreatedAt = now.AddDays(-40), CreatorId = U(s.Key), UserId = U(s.Key), Path = path, Tags = [TagMedia.Image, TagMedia.Profile], JsonData = new MediaJson() }, _ct);
		}

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();

		foreach (SeedUser s in Users) _tokens[s.Key] = ts.GenerateJwt(_users[s.Key]);
		_adminToken = ts.GenerateJwt(Core.App.Users.SystemAdmin);
	}

	private async Task SeedProfiles(DateTime now) {
		foreach (SeedUser s in Users) {
			for (int i = 0; i < s.Sports.Length; i++) {
				(TagSport sport, decimal level) = s.Sports[i];
				if (!_sports[sport].Tags.Contains(TagSport.Active)) continue;
				await db.Set<PlayerSportProfileEntity>().AddAsync(new PlayerSportProfileEntity {
					Id = SeedId($"profile:{s.Key}:{sport}"),
					CreatedAt = now.AddDays(-60),
					CreatorId = U(s.Key),
					Tags = i == 0 ? [TagPlayerSportProfile.Active, TagPlayerSportProfile.Primary] : [TagPlayerSportProfile.Active],
					Level = Scale(sport, level),
					UserId = U(s.Key),
					SportId = _sports[sport].Id,
					JsonData = new PlayerSportProfileJson()
				}, _ct);
			}
		}

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	private async Task SeedFollows(DateTime now) {
		HashSet<(string From, string To)> pairs = [];
		string[] demoFollows = ["farhad", "neda", "tara", "kaveh", "yasaman", "lucas", "sara", "omid", "owner", "organizer", "james", "elif", "reza", "ali", "mahsa"];
		foreach (string to in demoFollows) pairs.Add(("demo", to));
		foreach (SeedUser s in Users.Where(x => x.Key is not ("demo" or "behrooz" or "ghazal" or "hamed"))) pairs.Add((s.Key, "demo"));
		List<string> keys = Users.Select(x => x.Key).Where(x => x != "behrooz").ToList();
		foreach (string from in keys)
		foreach (string to in Shuffle(keys).Take(_random.Next(4, 9)))
			if (from != to)
				pairs.Add((from, to));
		pairs.Add(("behrooz", "demo"));
		pairs.Add(("behrooz", "neda"));

		foreach ((string from, string to) in pairs)
			await db.Set<FollowEntity>().AddAsync(new FollowEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddDays(-_random.Next(5, 55)).AddMinutes(-_random.Next(0, 1440)),
				CreatorId = U(from),
				UserId = U(to),
				Tags = [TagFollow.User],
				JsonData = new FollowJson()
			}, _ct);

		await db.Set<BlockEntity>().AddAsync(new BlockEntity { Id = Guid.CreateVersion7(), CreatedAt = now.AddDays(-4), CreatorId = U("demo"), BlockedUserId = U("behrooz"), Tags = [TagBlock.User], JsonData = new BlockJson() }, _ct);
		await db.Set<BlockEntity>().AddAsync(new BlockEntity { Id = Guid.CreateVersion7(), CreatedAt = now.AddDays(-9), CreatorId = U("kaveh"), BlockedUserId = U("behrooz"), Tags = [TagBlock.User], JsonData = new BlockJson() }, _ct);
		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	// ---------------- Venues ----------------

	private async Task SeedVenues(DateTime now) {
		for (int i = 0; i < Venues.Length; i++) {
			SeedVenue v = Venues[i];
			Guid venueId = SeedId($"venue:{v.Key}");
			await At(now.AddDays(-58 + i * 2), async () => {
				UResponse<Guid?> created = await venues.CreateVenue(new VenueCreateParams {
					Token = T(v.Owner),
					Id = venueId,
					Tags = v.PayAtVenue ? [v.Kind, TagVenue.PayAtVenue] : [v.Kind],
					Title = v.Title,
					Latitude = v.Lat,
					Longitude = v.Lng,
					Address = v.Address,
					PhoneNumber = v.Phone.IsNullOrEmpty() ? null : v.Phone,
					Country = "IR",
					City = v.City,
					Description = v.Description.IsNullOrEmpty() ? null : v.Description,
					Instagram = v.Instagram.IsNullOrEmpty() ? null : v.Instagram,
					Website = v.Instagram.IsNullOrEmpty() ? null : $"https://{v.Instagram.Replace('.', '-')}.ir",
					Whatsapp = v.Phone.IsNullOrEmpty() ? null : $"+98{v.Phone[1..]}",
					TimeZone = "Asia/Tehran",
					Currency = "IRT",
					Amenities = [..v.Amenities],
					OpeningHours = Enumerable.Range(0, 7).Select(d => new VenueOpeningHour { Day = d, Open = v.Open, Close = v.Close }).ToList(),
					CancellationFreeHours = v.FreeHours,
					CancellationPenaltyPercent = v.Penalty,
					AdminUserIds = v.Staff.Select(U).ToList()
				}, _ct);
				if (!Ok(created, $"venue {v.Key}")) return;

				foreach (SeedCourt c in v.Courts) {
					bool court = Ok(await venues.CreateCourt(new CourtCreateParams {
						Token = T(v.Owner),
						Id = SeedId($"court:{c.Key}"),
						Tags = [c.Indoor ? TagCourt.Indoor : TagCourt.Outdoor],
						Title = c.Title,
						VenueId = venueId,
						SportId = _sports[c.Sport].Id,
						PricePerHour = c.Price,
						SlotMinutes = c.Slot,
						Surface = c.Surface,
						Players = c.Players,
						PriceRules = c.Rules
					}, _ct), $"court {c.Key}");
					if (court) _courts[c.Key] = (v, c);
				}

				if (v.Status != TagVenue.Pending)
					Ok(await venues.UpdateVenue(new VenueUpdateParams {
						Token = _adminToken,
						Id = venueId,
						RemoveTags = [TagVenue.Pending],
						AddTags = [v.Status],
						RejectionReason = v.RejectionReason
					}, _ct), $"approve venue {v.Key}");
			});

			// Photos: a cover and two more of the place.
			if (v.Status == TagVenue.Rejected) continue;
			TagMedia[] shots = [TagMedia.Cover, TagMedia.Exterior, TagMedia.Interior];
			for (int n = 0; n < shots.Length; n++) {
				string path = $"venues/seed-{v.Key}-{n}.jpg";
				DrawVenue(path, v, n);
				await db.Set<MediaEntity>().AddAsync(new MediaEntity {
					Id = SeedId($"venue-media:{v.Key}:{n}"),
					CreatedAt = now.AddDays(-57 + i * 2),
					CreatorId = U(v.Owner),
					VenueId = venueId,
					Path = path,
					Tags = [TagMedia.Image, shots[n]],
					JsonData = new MediaJson()
				}, _ct);
			}
		}

		// Reviews
		(string Venue, string User, int Score, string Text)[] reviews = [
			("arena", "neda", 5, "بهترین زمین‌های پدل تهران. شیشه‌ها تمیز و نور عالی."),
			("arena", "kaveh", 4, "زمین‌ها عالی‌ان ولی ساعت‌های عصر خیلی شلوغه."),
			("arena", "lucas", 5, "Panoramic courts as good as Barcelona. Great staff!"),
			("arena", "tara", 4, "کافه‌اش خیلی خوبه، فقط پارکینگ کمه."),
			("arena", "sara", 5, "راکت اجاره‌ای هم کیفیت خوبی داشت."),
			("arena", "demo", 4, "برای تورنمنت‌ها بهترین جاست."),
			("enghelab", "emma", 5, "Lovely clay court, well maintained."),
			("enghelab", "james", 4, "Squash courts are proper size. Showers could be warmer."),
			("enghelab", "sara", 4, "خاک رس خیلی خوب نگهداری میشه."),
			("enghelab", "demo", 3, "رزرو در محل گاهی شلوغ میشه."),
			("cue", "pouya", 5, "میزهای اسنوکر استاندارد و ماهوت نو."),
			("cue", "parisa", 4, "فضای آرومی داره، قهوه‌اش هم خوبه."),
			("cue", "james", 5, "Best snooker tables I found in Tehran."),
			("garden", "farhad", 5, "بازی زیر درخت‌ها حال دیگه‌ای داره 🌳"),
			("garden", "yasaman", 4, "زمین روباز عصرها خنکه، عالیه."),
			("garden", "neda", 5, "میزبانی آرمان عالیه!"),
			("kish", "niloufar", 5, "غروب کیش و پدل؛ دیگه چی می‌خواین؟"),
			("kish", "omar", 4, "Beautiful sunset games. A bit windy."),
			("shiraz", "babak", 5, "چمن نسل ۴ واقعاً فرق داره."),
			("isfahan", "ghazal", 4, "زمین سرپوشیده‌اش خیلی خوبه.")
		];
		foreach ((string venueKey, string user, int score, string text) in reviews)
			await db.Set<CommentEntity>().AddAsync(new CommentEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddDays(-_random.Next(2, 40)),
				CreatorId = U(user),
				Tags = [TagComment.Released],
				Score = score,
				Description = text,
				VenueId = SeedId($"venue:{venueKey}"),
				JsonData = new CommentJson()
			}, _ct);

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	// ---------------- Scores ----------------

	private static readonly (int Hi, int Lo)[] TennisSets = [(6, 0), (6, 1), (6, 2), (6, 3), (6, 3), (6, 4), (6, 4), (7, 5), (7, 6)];

	/// <summary>Chance that side A wins, from the levels (plus a bonus for the seed's favourites).</summary>
	private double ChanceA(double strengthA, double strengthB) => 1 / (1 + Math.Exp(-(strengthA - strengthB) * 1.3));

	/// <summary>A valid result for the sport's rules: A wins when [aWins], a draw only where draws are allowed.</summary>
	private List<MatchSetScore> Score(TagSport sport, bool aWins, bool draw, bool pointsFormat, int pointsPerMatch, bool knockout) {
		MatchSetScore Set(bool aTakes, int hi, int lo) => aTakes ? new MatchSetScore { A = hi, B = lo } : new MatchSetScore { A = lo, B = hi };

		if (pointsFormat) {
			int winner = _random.Next(13, 19);
			return [Set(aWins, winner, pointsPerMatch - winner)];
		}

		switch (sport) {
			case TagSport.Padel:
			case TagSport.Tennis: {
				List<bool> order = _random.NextDouble() < 0.65 ? [true, true] : [true, false, true];
				return order.Select(w => {
					(int hi, int lo) = Pick(TennisSets);
					return Set(w == aWins, hi, lo);
				}).ToList();
			}
			case TagSport.Squash: {
				List<bool> order = _random.Next(3) switch { 0 => [true, true, true], 1 => [true, false, true, true], _ => [false, true, true, false, true] };
				return order.Select(w => _random.NextDouble() < 0.2 ? Set(w == aWins, 12, 10) : Set(w == aWins, 11, _random.Next(2, 10))).ToList();
			}
			case TagSport.Billiards:
				return [Set(aWins, 5, _random.Next(0, 5))];
			case TagSport.Snooker:
				return [Set(aWins, 3, _random.Next(0, 3))];
			default: {
				int goals = _random.Next(1, 6);
				if (draw && !knockout) return [new MatchSetScore { A = goals - 1, B = goals - 1 }];
				if (draw) return [new MatchSetScore { A = goals, B = goals }, Set(aWins, 5, 4)];
				return [Set(aWins, goals, _random.Next(0, goals))];
			}
		}
	}

	// ---------------- Tournaments ----------------

	private async Task<bool> CreateTournament(string key, string creator, TagSport sport, TagTournament format, TagTournament type, string title, DateTime start, int capacity, decimal fee,
		bool autoApprove, Action<TournamentCreateParams>? options = null, bool draft = false) {
		TournamentCreateParams p = new() {
			Token = T(creator),
			Id = SeedId($"tournament:{key}"),
			Tags = [format, type, ..autoApprove ? [TagTournament.AutoApprove] : Array.Empty<TagTournament>(), ..draft ? [TagTournament.Draft] : Array.Empty<TagTournament>()],
			Title = title,
			SportId = _sports[sport].Id,
			StartDate = start,
			Capacity = capacity,
			EntryFee = fee
		};
		options?.Invoke(p);
		return Ok(await sports.CreateTournament(p, _ct), $"tournament {key}");
	}

	private async Task<Guid?> Register(string tournament, string player, string? partner = null, string? team = null, bool pay = true) {
		UResponse<Guid?> r = await sports.RegisterTournamentEntry(new TournamentRegisterParams {
			Token = T(player),
			TournamentId = SeedId($"tournament:{tournament}"),
			PartnerEmail = partner == null ? null : Email(partner),
			Title = team,
			PayFromWallet = pay
		}, _ct);
		return Ok(r, $"register {player} in {tournament}") ? r.Result : null;
	}

	private async Task<bool> Generate(string tournament, string creator) =>
		Ok(await sports.GenerateTournamentMatches(new IdParams { Token = T(creator), Id = SeedId($"tournament:{tournament}") }, _ct), $"draw {tournament}");

	/// <summary>Plays the ready matches in order (new rounds appear as earlier ones finish) until [limit] results are in or none is left.</summary>
	private async Task Play(string tournament, string creator, TagSport sport, DateTime start, int? limit = null, string[]? favourites = null, int pointsPerMatch = 24) {
		Guid id = SeedId($"tournament:{tournament}");
		TournamentEntity t = await db.Set<TournamentEntity>().FirstAsync(x => x.Id == id, _ct);
		bool pointsFormat = t.Tags.Contains(TagTournament.Americano) || t.Tags.Contains(TagTournament.Mexicano);
		HashSet<string> fav = [..favourites ?? []];
		Dictionary<Guid, List<string>> players = (await db.Set<TournamentEntryEntity>().Where(x => x.TournamentId == id).Select(x => new { x.Id, Users = x.Users.Select(u => u.Id).ToList() }).ToListAsync(_ct))
			.ToDictionary(x => x.Id, x => x.Users.Where(_keyOf.ContainsKey).Select(u => _keyOf[u]).ToList());
		double Strength(Guid? entry) => entry == null || !players.TryGetValue(entry.Value, out List<string>? keys) || keys.Count == 0
			? 0
			: keys.Average(k => (double)LevelOf(k, sport)) + (keys.Any(fav.Contains) ? 2.5 : 0);

		int played = 0;
		while (limit == null || played < limit) {
			TournamentMatchEntity? m = await db.Set<TournamentMatchEntity>()
				.Where(x => x.TournamentId == id && x.EntryAId != null && x.EntryBId != null && !x.Tags.Contains(TagTournamentMatch.Finished) && !x.Tags.Contains(TagTournamentMatch.Bye))
				.OrderBy(x => x.Round).ThenBy(x => x.Order)
				.FirstOrDefaultAsync(_ct);
			if (m == null) break;

			double a = Strength(m.EntryAId) + Strength(m.PartnerAId), b = Strength(m.EntryBId) + Strength(m.PartnerBId);
			double chance = ChanceA(a, b);
			bool knockout = TournamentEngine.IsKnockout(m);
			bool draw = !pointsFormat && sport == TagSport.Football && Math.Abs(chance - 0.5) < 0.2 && _random.NextDouble() < 0.35;
			List<MatchSetScore> sets = Score(sport, _random.NextDouble() < chance, draw, pointsFormat, pointsPerMatch, knockout);

			bool ok = Ok(await sports.UpdateTournamentMatch(new TournamentMatchUpdateParams {
				Token = T(creator),
				Id = m.Id,
				Sets = sets,
				ScheduledAt = start.AddMinutes(played / 2 * 50),
				Court = sport is TagSport.Padel or TagSport.Tennis or TagSport.Squash ? $"{played % 2 + 1}" : null
			}, _ct), $"result in {tournament}");
			if (!ok) break;
			played++;
		}

		_result.TournamentMatches += played;
	}

	/// <summary>The first ready match goes live; the rest are given times from [from] on.</summary>
	private async Task ScheduleRest(string tournament, string creator, DateTime from, bool live) {
		Guid id = SeedId($"tournament:{tournament}");
		var waiting = await db.Set<TournamentMatchEntity>()
			.Where(x => x.TournamentId == id && !x.Tags.Contains(TagTournamentMatch.Finished) && !x.Tags.Contains(TagTournamentMatch.Bye))
			.OrderBy(x => x.Round).ThenBy(x => x.Order)
			.Select(x => new { x.Id, Ready = x.EntryAId != null && x.EntryBId != null })
			.ToListAsync(_ct);
		Guid? liveId = live ? waiting.FirstOrDefault(x => x.Ready)?.Id : null;
		for (int i = 0; i < waiting.Count; i++) {
			bool goLive = waiting[i].Id == liveId;
			Ok(await sports.UpdateTournamentMatch(new TournamentMatchUpdateParams {
				Token = T(creator),
				Id = waiting[i].Id,
				ScheduledAt = goLive ? DateTime.UtcNow.AddMinutes(-25) : from.AddMinutes(i * 60),
				Tags = goLive ? [TagTournamentMatch.Live] : null
			}, _ct), $"schedule {tournament}");
		}
	}

	/// <summary>The sport's past: tournaments and open games over the last six weeks, in date order so the levels move as they would have.</summary>
	private async Task SeedHistory(DateTime now) {
		List<(DateTime When, Func<Task> Action)> timeline = [];

		// Open games, two or three a day in the last six weeks, the demo player in at least one each week.
		(TagSport Sport, double Weight, int Capacity)[] mix = [(TagSport.Padel, 0.5, 4), (TagSport.Tennis, 0.18, 2), (TagSport.Squash, 0.12, 2), (TagSport.Billiards, 0.1, 2), (TagSport.Snooker, 0.05, 2), (TagSport.Football, 0.05, 6)];
		for (int day = 42; day >= 1; day--) {
			int games = _random.NextDouble() < 0.35 ? 2 : 1;
			for (int g = 0; g < games; g++) {
				double roll = _random.NextDouble(), sum = 0;
				(TagSport sport, double _, int capacity) = mix.First(x => (sum += x.Weight) >= roll || x == mix[^1]);
				if (sport == TagSport.Tennis && _random.NextDouble() < 0.4) capacity = 4;
				bool withDemo = LevelOf("demo", sport) > 0 && (day % 7 is 1 or 4 || _random.NextDouble() < 0.3);
				DateTime when = LocalAt(-day, 17 + g * 2.5 + _random.Next(0, 3) * 0.5);
				timeline.Add((when, () => PastOpenMatch(sport, capacity, withDemo, when)));
			}
		}

		timeline.Add((LocalAt(-30, 9), () => FinishedTennisLeague(LocalAt(-30, 9))));
		timeline.Add((LocalAt(-21, 15), () => FinishedPadelOpen(LocalAt(-21, 15))));
		timeline.Add((LocalAt(-16, 18), () => FinishedSwiss(LocalAt(-16, 18))));
		timeline.Add((LocalAt(-10, 17), () => FinishedAmericano(LocalAt(-10, 17))));
		timeline.Add((LocalAt(-7, 16), () => FinishedFootball(LocalAt(-7, 16))));
		timeline.Add((LocalAt(-5, 18), () => RunningSnookerLeague(LocalAt(-5, 18))));
		timeline.Add((LocalAt(-3, 10), () => RunningSquashMasters(LocalAt(-3, 10))));
		timeline.Add((LocalAt(-1, 19), () => RunningMexicano(LocalAt(-1, 19))));
		timeline.Add((DateTime.UtcNow.AddHours(-2), () => RunningPadelNightCup(DateTime.UtcNow.AddHours(-2))));

		foreach ((DateTime _, Func<Task> action) in timeline.OrderBy(x => x.When)) await action();
	}

	private async Task PastOpenMatch(TagSport sport, int capacity, bool withDemo, DateTime when) {
		List<string> roster = Roster(sport).Where(x => x != "demo").ToList();
		List<string> players = [..withDemo ? ["demo"] : Array.Empty<string>(), ..Shuffle(roster).Take(withDemo ? capacity - 1 : capacity)];
		if (players.Count < capacity) return;
		Guid id = Guid.CreateVersion7();
		string?[] titles = ["بازی عصرگاهی", "تمرین دوستانه", "بازی رقابتی", "دوبل آخر هفته", null, null];
		bool friendly = _random.NextDouble() < 0.15;
		SeedVenue? venue = Venues.Where(v => v.Status == TagVenue.Approved && v.Courts.Any(c => c.Sport == sport) && v.City == "Tehran").OrderBy(_ => _random.Next()).FirstOrDefault()
		                   ?? Venues.FirstOrDefault(v => v.Status == TagVenue.Approved && v.Courts.Any(c => c.Sport == sport));

		await At(when.AddDays(-2), async () => {
			if (!Ok(await sports.CreateOpenMatch(new OpenMatchCreateParams {
				    Token = T(players[0]),
				    Id = id,
				    Tags = [TagOpenMatch.Public, friendly ? TagOpenMatch.Friendly : TagOpenMatch.Competitive],
				    SportId = _sports[sport].Id,
				    StartAt = DateTime.UtcNow.AddHours(3),
				    DurationMinutes = sport == TagSport.Football ? 60 : 90,
				    Capacity = capacity,
				    PricePerPlayer = sport switch { TagSport.Padel => 300_000, TagSport.Football => 150_000, TagSport.Tennis => 250_000, _ => 120_000 },
				    VenueId = venue == null ? null : SeedId($"venue:{venue.Key}"),
				    Title = Pick(titles)
			    }, _ct), "past game")) return;
			foreach (string player in players.Skip(1)) Ok(await sports.JoinOpenMatch(new IdParams { Token = T(player), Id = id }, _ct), "join past game");
		});

		await db.Set<OpenMatchEntity>().Where(x => x.Id == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.StartAt, when), _ct);
		await At(when.AddMinutes(100), async () => {
			int half = capacity / 2;
			List<string> teamA = players.Take(half).ToList(), teamB = players.Skip(half).ToList();
			double chance = ChanceA(teamA.Average(k => (double)LevelOf(k, sport)), teamB.Average(k => (double)LevelOf(k, sport)));
			bool draw = sport == TagSport.Football && _random.NextDouble() < 0.2;
			Ok(await sports.SetOpenMatchResult(new OpenMatchResultParams {
				Token = T(players[0]),
				Id = id,
				TeamA = teamA.Select(U).ToList(),
				TeamB = teamB.Select(U).ToList(),
				Sets = Score(sport, _random.NextDouble() < chance, draw, false, 0, false)
			}, _ct), "past game result");
		});
	}

	private async Task FinishedTennisLeague(DateTime start) {
		await At(start.AddDays(-12), async () => {
			if (!await CreateTournament("tennis-league", "owner", TagSport.Tennis, TagTournament.RoundRobin, TagTournament.Singles, "لیگ بهاره تنیس", start, 6, 0, true, p => {
				    p.Description = "شش بازیکن، دور رفت. هر برد ۳ امتیاز.";
				    p.Venue = "باشگاه راکتی انقلاب";
				    p.Latitude = 35.7012;
				    p.Longitude = 51.3952;
				    p.Prize = "کاپ و راکت ویلسون";
			    })) return;
			foreach (string player in new[] { "demo", "sara", "omid", "farhad", "tara", "emma" }) await Register("tennis-league", player);
		});
		await At(start, async () => {
			if (await Generate("tennis-league", "owner")) await Play("tennis-league", "owner", TagSport.Tennis, start);
		});
	}

	private async Task FinishedPadelOpen(DateTime start) {
		await At(start.AddDays(-14), async () => {
			if (!await CreateTournament("padel-open", "organizer", TagSport.Padel, TagTournament.SingleElimination, TagTournament.Doubles, "اوپن پدل تهران", start, 8, 500_000, true, p => {
				    p.Description = "حذفی دونفره با بازی رده‌بندی. ورودیه از کیف پول پرداخت میشه.";
				    p.Venue = "پدل آرنا تهران";
				    p.Address = "تهران، ولیعصر، بالاتر از پارک‌وی";
				    p.Latitude = 35.7915;
				    p.Longitude = 51.4105;
				    p.Prize = "۲۰ میلیون تومان + کاپ";
				    p.ThirdPlaceMatch = true;
				    p.SuperTiebreak = true;
			    })) return;
			(string, string)[] pairs = [("demo", "farhad"), ("lucas", "kaveh"), ("omid", "tara"), ("neda", "yasaman"), ("reza", "ali"), ("sara", "amir"), ("babak", "parisa"), ("niloufar", "mina")];
			foreach ((string a, string b) in pairs) await Register("padel-open", a, b);
		});
		await At(start, async () => {
			if (await Generate("padel-open", "organizer")) await Play("padel-open", "organizer", TagSport.Padel, start, favourites: ["demo"]);
		});
	}

	private async Task FinishedSwiss(DateTime start) {
		await At(start.AddDays(-8), async () => {
			if (!await CreateTournament("cue-swiss", "owner", TagSport.Billiards, TagTournament.Swiss, TagTournament.Singles, "سوئیسی کیو کلاب", start, 6, 150_000, true, p => {
				    p.Description = "سه دور سوئیسی، هر بازی تا ۵ رک.";
				    p.Venue = "کیو کلاب ونک";
				    p.Rounds = 3;
			    })) return;
			foreach (string player in new[] { "demo", "reza", "pouya", "parisa", "siavash", "james" }) await Register("cue-swiss", player);
		});
		await At(start, async () => {
			if (await Generate("cue-swiss", "owner")) await Play("cue-swiss", "owner", TagSport.Billiards, start);
		});
	}

	private async Task FinishedAmericano(DateTime start) {
		await At(start.AddDays(-5), async () => {
			if (!await CreateTournament("americano", "organizer", TagSport.Padel, TagTournament.Americano, TagTournament.Singles, "آمریکانوی جمعه", start, 8, 0, true, p => {
				    p.Description = "هر دور با هم‌تیمی جدید. ۲۴ امتیاز در هر بازی.";
				    p.Venue = "باغ پدل کیانی";
				    p.PointsPerMatch = 24;
			    })) return;
			foreach (string player in new[] { "demo", "neda", "sara", "shirin", "yasaman", "tara", "mahsa", "sofia" }) await Register("americano", player);
		});
		await At(start, async () => {
			if (await Generate("americano", "organizer")) await Play("americano", "organizer", TagSport.Padel, start);
		});
	}

	private async Task FinishedFootball(DateTime start) {
		await At(start.AddDays(-10), async () => {
			if (!await CreateTournament("football", "organizer", TagSport.Football, TagTournament.RoundRobin, TagTournament.Team, "جام فوتبال پنج‌نفره", start, 4, 1_000_000, true, p => {
				    p.Description = "چهار تیم، دوره‌ای. برد ۳، مساوی ۱.";
				    p.Venue = "فوتبال پنج‌نفره شیراز";
				    p.Prize = "کاپ قهرمانی";
			    })) return;
			(string, string)[] teams = [("reza", "مهاجمان تهران"), ("babak", "شیرهای آزادی"), ("ali", "ستاره‌های شمال"), ("omar", "Dubai Falcons")];
			foreach ((string captain, string team) in teams) await Register("football", captain, team: team);
		});
		await At(start, async () => {
			if (await Generate("football", "organizer")) await Play("football", "organizer", TagSport.Football, start);
		});
	}

	private async Task RunningSnookerLeague(DateTime start) {
		await At(start.AddDays(-6), async () => {
			if (!await CreateTournament("snooker-box", "owner", TagSport.Snooker, TagTournament.Ladder, TagTournament.Singles, "لیگ جعبه‌ای اسنوکر · پاییز", start, 6, 0, true, p => {
				    p.Description = "جعبه‌های سه‌نفره؛ اول هر جعبه بالا می‌ره.";
				    p.Venue = "کیو کلاب ونک";
				    p.BoxSize = 3;
			    })) return;
			foreach (string player in new[] { "demo", "babak", "pouya", "hamed", "siavash", "james" }) await Register("snooker-box", player);
		});
		await At(start, async () => {
			if (!await Generate("snooker-box", "owner")) return;
			await Play("snooker-box", "owner", TagSport.Snooker, start, 4);
			await ScheduleRest("snooker-box", "owner", LocalAt(1, 19), false);
		});
	}

	private async Task RunningSquashMasters(DateTime start) {
		await At(start.AddDays(-9), async () => {
			if (!await CreateTournament("squash-masters", "organizer", TagSport.Squash, TagTournament.GroupsKnockout, TagTournament.Singles, "مسترز اسکواش", start, 8, 300_000, true, p => {
				    p.Description = "دو گروه چهارنفره، دو نفر اول هر گروه به نیمه‌نهایی می‌رسن.";
				    p.Venue = "باشگاه راکتی انقلاب";
				    p.GroupCount = 2;
				    p.AdvancePerGroup = 2;
			    })) return;
			foreach (string player in new[] { "demo", "kaveh", "leila", "yasaman", "amir", "ghazal", "omar", "james" }) await Register("squash-masters", player);
		});
		await At(start, async () => {
			if (!await Generate("squash-masters", "organizer")) return;
			await Play("squash-masters", "organizer", TagSport.Squash, start, 13, ["demo"]);
			await ScheduleRest("squash-masters", "organizer", LocalAt(1, 18), true);
		});
	}

	private async Task RunningMexicano(DateTime start) {
		await At(start.AddDays(-4), async () => {
			if (!await CreateTournament("mexicano", "organizer", TagSport.Padel, TagTournament.Mexicano, TagTournament.Singles, "مکزیکانوی دوشنبه‌ها", start, 8, 0, true, p => {
				    p.Description = "چهار دور؛ هر دور بر اساس جدول، هم‌تیمی‌ها عوض می‌شن.";
				    p.Venue = "پدل آرنا تهران";
				    p.Rounds = 4;
				    p.PointsPerMatch = 24;
			    })) return;
			foreach (string player in new[] { "demo", "kaveh", "mina", "babak", "shirin", "niloufar", "parisa", "ghazal" }) await Register("mexicano", player);
		});
		await At(start, async () => {
			if (!await Generate("mexicano", "organizer")) return;
			await Play("mexicano", "organizer", TagSport.Padel, start, 3);
			await ScheduleRest("mexicano", "organizer", DateTime.UtcNow.AddMinutes(40), true);
		});
	}

	private async Task RunningPadelNightCup(DateTime start) {
		await At(start.AddDays(-6), async () => {
			if (!await CreateTournament("night-cup", "owner", TagSport.Padel, TagTournament.DoubleElimination, TagTournament.Doubles, "جام شبانه پدل", start, 6, 400_000, true, p => {
				    p.Description = "حذفی دوگانه: هر تیم با دو باخت حذف میشه. نتایج زنده.";
				    p.Venue = "پدل آرنا تهران";
				    p.Latitude = 35.7915;
				    p.Longitude = 51.4105;
				    p.Prize = "۱۲ میلیون تومان";
			    })) return;
			(string, string)[] pairs = [("neda", "omid"), ("kaveh", "yasaman"), ("lucas", "sofia"), ("reza", "tara"), ("ali", "sara"), ("omar", "elif")];
			foreach ((string a, string b) in pairs) await Register("night-cup", a, b);
		});
		await At(start, async () => {
			if (!await Generate("night-cup", "owner")) return;
			await Play("night-cup", "owner", TagSport.Padel, start, 5);
			await ScheduleRest("night-cup", "owner", DateTime.UtcNow.AddMinutes(50), true);
		});
	}

	// ---------------- What's coming ----------------

	private async Task SeedUpcoming(DateTime now) {
		// Registration open, with approval: some entries approved, two waiting, one rejected (and refunded).
		if (await CreateTournament("autumn-cup", "organizer", TagSport.Padel, TagTournament.SingleElimination, TagTournament.Doubles, "جام پاییزه پدل", LocalAt(9, 16), 16, 600_000, false, p => {
			    p.Description = "حذفی ۱۶ تیمی با بازی رده‌بندی. ثبت‌نام با تأیید برگزارکننده.";
			    p.Venue = "پدل آرنا تهران";
			    p.Address = "تهران، ولیعصر، بالاتر از پارک‌وی";
			    p.Latitude = 35.7915;
			    p.Longitude = 51.4105;
			    p.Prize = "۳۰ میلیون تومان + کاپ";
			    p.ThirdPlaceMatch = true;
			    p.MinLevel = Scale(TagSport.Padel, 2.5m);
			    p.MaxLevel = Scale(TagSport.Padel, 6.5m);
		    })) {
			(string, string)[] pairs = [("demo", "yasaman"), ("lucas", "farhad"), ("neda", "tara"), ("kaveh", "omid"), ("sara", "reza"), ("elif", "sofia"), ("mahsa", "shirin")];
			List<Guid?> entries = [];
			foreach ((string a, string b) in pairs) entries.Add(await Register("autumn-cup", a, b));
			for (int i = 0; i < entries.Count; i++) {
				if (entries[i] == null || i is 4 or 5) continue;
				Ok(await sports.UpdateTournamentEntry(new TournamentEntryUpdateParams { Token = T("organizer"), Id = entries[i]!.Value, Tags = [i == 6 ? TagTournamentEntry.Rejected : TagTournamentEntry.Approved], Seed = i < 3 ? i + 1 : null }, _ct), "approve entry");
			}
		}

		if (await CreateTournament("tennis-day", "owner", TagSport.Tennis, TagTournament.RoundRobin, TagTournament.Singles, "روز باز تنیس", LocalAt(5, 9), 8, 0, true, p => {
			    p.Description = "رایگان و دوستانه، برای همه‌ی سطح‌ها.";
			    p.Venue = "باشگاه راکتی انقلاب";
			    p.Unrated = true;
		    }))
			foreach (string player in new[] { "sara", "emma", "elif", "shirin" }) await Register("tennis-day", player);

		if (await CreateTournament("squash-beginners", "owner", TagSport.Squash, TagTournament.SingleElimination, TagTournament.Singles, "جام نوآموزان اسکواش", LocalAt(12, 17), 8, 200_000, true, p => {
			    p.Description = "فقط برای سطح ۴٫۵ و پایین‌تر.";
			    p.Venue = "باشگاه راکتی انقلاب";
			    p.MaxLevel = Scale(TagSport.Squash, 4.5m);
		    }))
			foreach (string player in new[] { "ghazal", "organizer", "leila" }) await Register("squash-beginners", player);

		// The demo player's own draft.
		await CreateTournament("garden-weekend", "demo", TagSport.Padel, TagTournament.RoundRobin, TagTournament.Doubles, "جام آخر هفته باغ کیانی", LocalAt(20, 10), 6, 250_000, true, p => {
			p.Description = "پیش‌نویس — هنوز منتشر نشده.";
			p.Venue = "باغ پدل کیانی";
		}, true);

		// Open games: one starting within the hour (its reminder comes from the background service), some to join, a full one,
		// a private one waiting for approval, an invitation and a challenge for the demo player.
		DateTime soon = DateTime.UtcNow.AddMinutes(55);
		await OpenGame("p-soon", "neda", TagSport.Padel, soon, 4, "arena", "بازی امروز عصر", ["demo", "tara", "farhad"]);
		Guid? booking = await Book("demo", "arena-1", 1, 8, split: ["tara", "neda", "farhad"], notes: "بازی دوستانه با بچه‌ها");
		if (booking != null) Ok(await venues.PayBookingShare(new IdParams { Token = T("tara"), Id = booking.Value }, _ct), "pay share");
		await OpenGame("p-demo", "demo", TagSport.Padel, SlotUtc(Venues[0], Venues[0].Courts[0], 1, 8), 4, "arena", "دوبل فردا شب · زمین پانوراما", ["tara"], booking: booking,
			min: Scale(TagSport.Padel, 3.5m), max: Scale(TagSport.Padel, 5.5m));
		await OpenGame("p-garden", "kaveh", TagSport.Padel, LocalAt(2, 19.5), 4, "garden", "دنبال یک نفر برای دوبل", ["yasaman", "omid"]);
		await OpenGame("p-private", "farhad", TagSport.Padel, LocalAt(3, 20), 4, "arena", "بازی سطح بالا (خصوصی)", [], priv: true, requests: ["demo", "tara"]);
		await OpenGame("t-challenge", "lucas", TagSport.Tennis, LocalAt(4, 10), 2, "enghelab", "Challenge: best of three?", [], challenge: true, invited: ["demo"]);
		await OpenGame("t-full", "omid", TagSport.Tennis, LocalAt(2, 18), 4, "enghelab", "دوبل تنیس دوستانه", ["sara", "tara", "mahsa"], friendly: true);
		await OpenGame("b-open", "pouya", TagSport.Billiards, LocalAt(1, 21), 2, "cue", null, []);
		await OpenGame("s-invite", "yasaman", TagSport.Squash, LocalAt(2, 8), 2, "enghelab", "اسکواش صبح زود", [], invited: ["demo"]);
		await OpenGame("f-open", "babak", TagSport.Football, LocalAt(5, 20), 10, "shiraz", "فوتبال پنج‌نفره شب جمعه", ["reza", "ali", "amir", "hamed", "omid"]);
		await OpenGame("p-park", "amir", TagSport.Padel, LocalAt(6, 8), 4, null, "پدل صبح جمعه در پارک", [], place: "پارک ملت، زمین‌های عمومی", lat: 35.7792, lng: 51.4210);
		await OpenGame("p-kish", "niloufar", TagSport.Padel, LocalAt(3, 18), 4, "kish", "غروب کیش 🌅", ["mina"]);
	}

	private async Task<Guid> OpenGame(string key, string creator, TagSport sport, DateTime start, int capacity, string? venue, string? title, string[] joiners, bool priv = false,
		bool challenge = false, bool friendly = false, string[]? invited = null, string[]? requests = null, Guid? booking = null, decimal? min = null, decimal? max = null,
		string? place = null, double? lat = null, double? lng = null) {
		Guid id = SeedId($"game:{key}");
		if (!Ok(await sports.CreateOpenMatch(new OpenMatchCreateParams {
			    Token = T(creator),
			    Id = id,
			    Tags = [priv ? TagOpenMatch.Private : TagOpenMatch.Public, friendly ? TagOpenMatch.Friendly : TagOpenMatch.Competitive, ..challenge ? [TagOpenMatch.Challenge] : Array.Empty<TagOpenMatch>()],
			    SportId = _sports[sport].Id,
			    StartAt = start,
			    DurationMinutes = sport == TagSport.Football ? 60 : 90,
			    Capacity = capacity,
			    MinLevel = min,
			    MaxLevel = max,
			    PricePerPlayer = sport switch { TagSport.Padel => 300_000, TagSport.Football => 150_000, TagSport.Tennis => 250_000, _ => 120_000 },
			    VenueId = venue == null ? null : SeedId($"venue:{venue}"),
			    BookingId = booking,
			    Title = title,
			    Description = challenge ? "Loser buys the coffee ☕" : null,
			    Place = place,
			    Latitude = lat,
			    Longitude = lng,
			    InvitedUserIds = (invited ?? []).Select(U).ToList()
		    }, _ct), $"game {key}"))
			return id;
		foreach (string player in joiners.Concat(requests ?? [])) Ok(await sports.JoinOpenMatch(new IdParams { Token = T(player), Id = id }, _ct), $"join game {key}");
		return id;
	}

	// ---------------- Bookings ----------------

	private async Task<Guid?> Book(string user, string court, int day, int slot, int slots = 1, bool wallet = true, string[]? split = null, string? notes = null) {
		if (!_courts.TryGetValue(court, out (SeedVenue Venue, SeedCourt Court) vc)) return null;
		if (slot < 0 || slot + slots > SlotsPerDay(vc.Venue, vc.Court) || !_usedSlots.Add((court, day, slot))) return null;
		UResponse<BookingResponse?> r = await venues.CreateBooking(new BookingCreateParams {
			Token = T(user),
			CourtId = SeedId($"court:{court}"),
			StartAt = SlotUtc(vc.Venue, vc.Court, day, slot),
			DurationMinutes = vc.Court.Slot * slots,
			PayFromWallet = wallet,
			SplitWithUserIds = (split ?? []).Select(U).ToList(),
			Notes = notes
		}, _ct);
		return Ok(r, $"booking {court} by {user}") ? r.Result?.Id : null;
	}

	/// <summary>A booking in the past: made for a free future slot, moved to its day, then closed by the venue.</summary>
	private async Task PastBooking(string user, string court, int daysAgo, int slot, TagBooking end = TagBooking.Completed, string[]? split = null) {
		if (!_courts.TryGetValue(court, out (SeedVenue Venue, SeedCourt Court) vc) || !_usedSlots.Add((court, -daysAgo, slot))) return;
		DateTime start = SlotUtc(vc.Venue, vc.Court, -daysAgo, slot);
		Guid? id = null;
		await At(start.AddDays(-2), async () => {
			id = await Book(user, court, _tempDay++, 0, split: split);
			if (id == null) return;
			foreach (string friend in split ?? []) Ok(await venues.PayBookingShare(new IdParams { Token = T(friend), Id = id.Value }, _ct), "pay share");
		});
		if (id == null) return;
		await db.Set<BookingEntity>().Where(x => x.Id == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.StartAt, start).SetProperty(x => x.EndAt, start.AddMinutes(vc.Court.Slot)), _ct);
		await At(start.AddMinutes(vc.Court.Slot + 30), async () =>
			Ok(await venues.UpdateBooking(new BookingUpdateParams { Token = T(vc.Venue.Owner), Id = id.Value, Tags = [end] }, _ct), "close booking"));
	}

	private async Task SeedBookings(DateTime now) {
		// The demo player's own: a pay-at-venue one, one shared with them (their share unpaid), two cancelled (one late, with a penalty).
		await Book("demo", "enghelab-t1", 3, 2, wallet: false, notes: "تمرین سرویس");
		await Book("farhad", "arena-2", 2, 9, split: ["demo"], notes: "آرمان سهمت رو بزن 😄");
		Guid? early = await Book("demo", "arena-4", 5, 6);
		if (early != null) Ok(await venues.CancelBooking(new BookingCancelParams { Token = T("demo"), Id = early.Value, Reason = "برنامه‌ام عوض شد" }, _ct), "cancel booking");
		Guid? late = await Book("demo", "cue-b1", 1, 2);
		if (late != null) Ok(await venues.CancelBooking(new BookingCancelParams { Token = T("demo"), Id = late.Value, Reason = "دیر رسیدم" }, _ct), "late cancel");

		await PastBooking("demo", "arena-3", 6, 7, split: ["farhad"]);
		await PastBooking("demo", "enghelab-s1", 13, 12);
		await PastBooking("demo", "cue-s1", 20, 6, TagBooking.NoShow);
		await PastBooking("demo", "arena-1", 27, 8);

		// Busy weeks at every club (for the owners' calendars and dashboards), the demo player's garden included.
		string[] players = Users.Select(x => x.Key).Where(x => x is not ("demo" or "behrooz")).ToArray();
		foreach ((string court, (SeedVenue venue, SeedCourt c)) in _courts.ToList()) {
			if (venue.Status != TagVenue.Approved) continue;
			int slots = SlotsPerDay(venue, c);
			int future = venue.Key is "arena" or "garden" ? 6 : 3, past = venue.Key is "arena" or "garden" ? 7 : 3;
			for (int i = 0; i < future; i++) {
				string user = Pick(players);
				if (user == venue.Owner) continue;
				string[]? split = c.Players > 2 && _random.NextDouble() < 0.4 ? Shuffle(players.Where(x => x != user && x != venue.Owner)).Take(_random.Next(1, 4)).ToArray() : null;
				await Book(user, court, _random.Next(1, 9), _random.Next(slots / 3, slots), wallet: !venue.PayAtVenue || _random.NextDouble() < 0.7, split: split);
			}

			for (int i = 0; i < past; i++) {
				string user = Pick(players);
				if (user == venue.Owner) continue;
				await PastBooking(user, court, _random.Next(1, 29), _random.Next(slots / 3, slots), _random.NextDouble() < 0.1 ? TagBooking.NoShow : TagBooking.Completed);
			}
		}

		// A booking at the demo player's garden that its player cancelled.
		Guid? cancelled = await Book("neda", "garden-1", 4, 6);
		if (cancelled != null) Ok(await venues.CancelBooking(new BookingCancelParams { Token = T("neda"), Id = cancelled.Value, Reason = "مسافرت" }, _ct), "cancel garden booking");
	}

	// ---------------- Posts, stories, reports ----------------

	private async Task SeedPosts(DateTime now) {
		Guid padelOpen = SeedId("tournament:padel-open");
		(string User, double HoursAgo, string Text, string? Image, string? LinkType, Guid? LinkId, bool Followers)[] posts = [
			("demo", 500, "قهرمان اوپن پدل تهران شدیم! 🏆 مرسی فرهاد، بهترین هم‌تیمی دنیا.", "trophy", "tournament", padelOpen, false),
			("farhad", 498, "چه فینالی بود! آرمان امروز ترکوند 🔥", null, "tournament", padelOpen, false),
			("organizer", 480, "عکس‌های اوپن پدل تهران رو گذاشتیم. ماه بعد جام پاییزه، ثبت‌نام بازه.", "padel", "tournament", SeedId("tournament:autumn-cup"), false),
			("lucas", 300, "First week in Tehran. The padel scene here is amazing. Who wants to play?", "padel", null, null, false),
			("neda", 260, "صبح ساعت ۷ و زمین خالی. بهترین حس دنیا ☀️", "court", null, null, false),
			("demo", 230, "باغ پدل کیانی آماده‌ست! زمین روباز زیر درخت‌ها 🌳 رزرو از توی اپ.", "court", "venue", SeedId("venue:garden"), false),
			("tara", 200, "کسی برای چهارشنبه عصر دوبل هست؟", null, null, null, false),
			("james", 180, "Snooker box league started. Tough group!", "table", "tournament", SeedId("tournament:snooker-box"), false),
			("kaveh", 150, "بک‌هند رو بالاخره درست کردم. مرسی از کلاس فرهاد.", null, null, null, true),
			("owner", 140, "زمین‌های ۱ و ۲ پدل آرنا نورپردازی جدید گرفتن 💡", "court", "venue", SeedId("venue:arena"), false),
			("sara", 120, "اولین تورنمنت پدلم بود، کلی یاد گرفتم 💪", "padel", null, null, false),
			("demo", 96, "سه هفته پشت سر هم بازی! نشان «همیشگی» گرفتم 😎", null, null, null, false),
			("omar", 90, "Dubai Falcons are ready for the next five-a-side cup ⚽️", "pitch", "tournament", SeedId("tournament:football"), false),
			("yasaman", 72, "اسکواش صبح زود + قهوه = بهترین شروع روز", null, null, null, false),
			("pouya", 60, "مجموعه‌ی پدل مشهد به زودی! منتظر تأیید هستیم 🙏", "court", null, null, false),
			("elif", 50, "Tehran sunsets after a padel match 🧡", "padel", null, null, false),
			("mahsa", 40, "مرسی آرمان که منو با پدل آشنا کردی!", null, null, null, false),
			("babak", 30, "تیم شیرهای آزادی دنبال یک بازیکن دفاع هست. پیام بدید.", null, null, null, false),
			("demo", 20, "کی پایه‌ست فردا شب؟ یک جا خالیه 👇", null, "openMatch", SeedId("game:p-demo"), false),
			("farhad", 12, "نکته‌ی امروز: توی پدل، بالای سر بازی کن نه جلوی سینه.", null, null, null, false),
			("behrooz", 10, "راکت اصل نصف قیمت!!! فقط امروز!!! دایرکت", null, null, null, false),
			("neda", 5, "جام شبانه پدل امشب زنده‌ست! بیاید تماشا 🎾", "padel", "tournament", SeedId("tournament:night-cup"), false),
			("emma", 3, "Booked my first court through the app, super easy.", null, null, null, false)
		];

		Dictionary<int, Guid> postIds = new();
		string[] likers = Users.Select(x => x.Key).Where(x => x != "behrooz").ToArray();
		string[] replies = ["عالیه! 👏", "تبریک 🎉", "منم پایه‌ام", "کی دوباره بازی کنیم؟", "Great job!", "👏👏👏", "دمت گرم", "Let's play next week!", "چه خوب 😍", "منو هم خبر کن"];
		for (int i = 0; i < posts.Length; i++) {
			(string user, double hoursAgo, string text, string? image, string? linkType, Guid? linkId, bool followers) = posts[i];
			DateTime at = now.AddHours(-hoursAgo);
			Guid id = SeedId($"post:{i}");
			postIds[i] = id;
			List<string> liked = user == "behrooz" ? ["shirin"] : Shuffle(likers.Where(x => x != user)).Take(_random.Next(2, 14)).ToList();
			await db.Set<PostEntity>().AddAsync(new PostEntity {
				Id = id,
				CreatedAt = at,
				CreatorId = U(user),
				Tags = [TagPost.Post, followers ? TagPost.Followers : TagPost.Public, ..user == "behrooz" ? [TagPost.Hidden] : Array.Empty<TagPost>()],
				Text = text,
				JsonData = new PostJson {
					LinkType = linkType,
					LinkId = linkId,
					Reactions = liked.Select(x => new PostReaction { UserId = U(x), Tag = TagReaction.Like }).ToList()
				}
			}, _ct);
			if (image != null) {
				string path = $"posts/seed-{i}.jpg";
				DrawPost(path, image, i);
				await db.Set<MediaEntity>().AddAsync(new MediaEntity { Id = SeedId($"post-media:{i}"), CreatedAt = at, CreatorId = U(user), PostId = id, Path = path, Tags = [TagMedia.Image], JsonData = new MediaJson() }, _ct);
			}

			int replyCount = user == "behrooz" ? 0 : _random.Next(0, 5);
			for (int r = 0; r < replyCount; r++) {
				string replier = Pick(likers.Where(x => x != user).ToList());
				DateTime replyAt = at.AddMinutes(_random.Next(5, (int)Math.Max(10, Math.Min(hoursAgo * 60 - 5, 600))));
				await db.Set<PostEntity>().AddAsync(new PostEntity {
					Id = SeedId($"post:{i}:reply:{r}"),
					CreatedAt = replyAt,
					CreatorId = U(replier),
					Tags = [TagPost.Comment, TagPost.Public],
					Text = Pick(replies),
					ParentId = id,
					JsonData = new PostJson { Reactions = _random.NextDouble() < 0.5 ? [new PostReaction { UserId = U(user), Tag = TagReaction.Like }] : [] }
				}, _ct);
				if (user == "demo") await Notify("demo", replier, "notifNewReply", Name(replier), "post", id, replyAt, TagNotification.Social);
			}

			if (user == "demo")
				foreach (string liker in liked.Take(4))
					await Notify("demo", liker, "notifNewReaction", Name(liker), "post", id, at.AddMinutes(_random.Next(3, 300)), TagNotification.Social);
		}

		// Stories: some the demo player has seen, some not, and their own.
		(string User, double HoursAgo, string Image, bool Seen)[] stories = [
			("farhad", 2, "padel", false), ("neda", 4, "court", true), ("tara", 6, "padel", false), ("lucas", 9, "court", false),
			("kaveh", 13, "padel", true), ("organizer", 18, "trophy", false), ("demo", 3, "court", false)
		];
		for (int i = 0; i < stories.Length; i++) {
			(string user, double hoursAgo, string image, bool seen) = stories[i];
			DateTime at = now.AddHours(-hoursAgo);
			Guid id = SeedId($"story:{i}");
			List<Guid> viewers = Shuffle(likers.Where(x => x != user && x != "demo")).Take(_random.Next(3, 12)).Select(U).ToList();
			if (seen) viewers.Add(U("demo"));
			await db.Set<PostEntity>().AddAsync(new PostEntity {
				Id = id,
				CreatedAt = at,
				CreatorId = U(user),
				Tags = [TagPost.Story, TagPost.Public],
				ExpiresAt = at.AddHours(24),
				JsonData = new PostJson { ViewerIds = viewers }
			}, _ct);
			string path = $"posts/seed-story-{i}.jpg";
			DrawPost(path, image, 40 + i);
			await db.Set<MediaEntity>().AddAsync(new MediaEntity { Id = SeedId($"story-media:{i}"), CreatedAt = at, CreatorId = U(user), PostId = id, Path = path, Tags = [TagMedia.Image], JsonData = new MediaJson() }, _ct);
		}

		// Reports for the moderators: the spam post and its author waiting, older ones handled.
		Guid spamPost = postIds[20];
		(string User, TagReport Kind, TagReport Status, Guid Target, string Reason, string? Note, double HoursAgo)[] reports = [
			("demo", TagReport.Post, TagReport.Pending, spamPost, "تبلیغات و اسپم", null, 9),
			("neda", TagReport.Post, TagReport.Pending, spamPost, "کلاهبرداری", null, 8),
			("kaveh", TagReport.User, TagReport.Pending, U("behrooz"), "پیام‌های تبلیغاتی مکرر", null, 30),
			("sara", TagReport.User, TagReport.Resolved, U("behrooz"), "حساب جعلی", "هشدار داده شد.", 200),
			("tara", TagReport.Post, TagReport.Dismissed, postIds[9], "تبلیغ مجموعه", "پست مجموعه‌ی ورزشی، مشکلی نداره.", 120),
			("omid", TagReport.Venue, TagReport.Pending, SeedId("venue:lavasan"), "نشانی اشتباه", null, 50)
		];
		foreach ((string user, TagReport kind, TagReport status, Guid target, string reason, string? note, double hoursAgo) in reports)
			await db.Set<ReportEntity>().AddAsync(new ReportEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddHours(-hoursAgo),
				CreatorId = U(user),
				Tags = [kind, status],
				TargetId = target,
				Reason = reason,
				JsonData = new ReportJson { Note = note }
			}, _ct);

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	private async Task Notify(string to, string from, string key, string? subject, string? linkType, Guid? linkId, DateTime at, TagNotification kind) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = at,
			CreatorId = U(from),
			UserId = U(to),
			Tags = [kind, TagNotification.Unread],
			JsonData = new NotificationJson { Detail1 = key, Detail2 = subject ?? "", LinkType = linkType, LinkId = linkId }
		}, _ct);

	// ---------------- Chats ----------------

	private async Task SeedChats(DateTime now) {
		List<(string Key, string? Title, string[] Members, (string From, string Text, double MinutesAgo, string? LinkType, Guid? LinkId)[] Messages, int UnreadForDemo)> chats = [
			("farhad", null, ["demo", "farhad"], [
				("farhad", "سلام آرمان، فردا شب هستی؟", 2900, null, null),
				("demo", "سلام! آره، زمین ۱ رو رزرو کردم", 2890, null, null),
				("farhad", "عالیه. سهم منو از کیف پول پرداخت می‌کنم", 2880, null, null),
				("demo", "مرسی 🙏 تارا و ندا هم میان", 2870, null, null),
				("farhad", "پس مسترز اسکواش رو چی‌کار می‌کنی؟ نیمه‌نهایی فرداست", 600, null, null),
				("demo", "می‌رم ببینم چی میشه 😅", 590, null, null),
				("farhad", "این بازی رو دیدی؟", 45, "openMatch", SeedId("game:p-private")),
				("farhad", "خصوصیه ولی درخواستت رو قبول می‌کنم", 44, null, null),
				("farhad", "فقط زودتر بیا گرم کنیم", 12, null, null)
			], 3),
			("tara", null, ["demo", "tara"], [
				("tara", "سهمم رو برای رزرو فردا زدم ✅", 1500, null, null),
				("demo", "دمت گرم!", 1490, null, null),
				("tara", "راستی امروز عصر هم بازی هست، بیا", 300, "openMatch", SeedId("game:p-soon")),
				("demo", "اومدم توش 👍", 280, null, null)
			], 0),
			("lucas", null, ["demo", "lucas"], [
				("lucas", "Hey Arman! Saw your Padel Open win. Congrats!", 6000, null, null),
				("demo", "Thanks Lucas! You guys were tough in the semis.", 5990, null, null),
				("lucas", "Up for a tennis challenge this week?", 200, "openMatch", SeedId("game:t-challenge")),
				("lucas", "Best of three, loser buys the coffee ☕", 199, null, null)
			], 2),
			("owner", null, ["demo", "owner"], [
				("demo", "سلام آقای علوی، برای جام پاییزه زمین ۱ و ۲ رو از ساعت ۱۶ نگه می‌دارید؟", 4000, null, null),
				("owner", "سلام، حتماً. کیمیا هماهنگ می‌کنه.", 3950, null, null),
				("owner", "این هم لینک تورنمنت", 3940, "tournament", SeedId("tournament:autumn-cup"))
			], 0),
			("crew", "اکیپ پدل جمعه‌ها", ["demo", "neda", "yasaman", "tara", "farhad"], [
				("neda", "بچه‌ها این جمعه کجا بازی کنیم؟", 3000, null, null),
				("yasaman", "باغ کیانی؟ 🌳", 2990, null, null),
				("demo", "قدمتون روی چشم، زمین روباز رو نگه می‌دارم", 2980, "venue", SeedId("venue:garden")),
				("tara", "من ساعت ۸ صبح هستم", 2970, null, null),
				("farhad", "منم", 2960, null, null),
				("neda", "امشب جام شبانه هم هست، کسی میاد تماشا؟", 120, "tournament", SeedId("tournament:night-cup")),
				("yasaman", "من میام!", 100, null, null),
				("tara", "منم بعد از بازی میام", 60, null, null)
			], 3),
			("open", "بازیکنان اوپن پدل تهران", ["organizer", "demo", "farhad", "lucas", "kaveh", "omid", "tara", "neda", "yasaman"], [
				("organizer", "سلام به همه! قرعه‌کشی انجام شد، جدول توی اپ هست.", 31000, "tournament", padelOpenId),
				("organizer", "لطفاً ۳۰ دقیقه قبل از بازی برسید.", 30900, null, null),
				("kaveh", "زمین ۱ یا ۲؟", 30800, null, null),
				("organizer", "برنامه‌ی زمین‌ها توی جدول هست", 30790, null, null),
				("organizer", "تبریک به آرمان و فرهاد، قهرمان‌های اوپن! 🏆", 29000, null, null)
			], 0),
			("kaveh-yasaman", null, ["kaveh", "yasaman"], [
				("kaveh", "بازی پنجشنبه رو ساختم، بیا", 900, "openMatch", SeedId("game:p-garden")),
				("yasaman", "اومدم ✌️", 880, null, null)
			], 0)
		];

		foreach ((string key, string? title, string[] members, var messages, int unread) in chats) {
			Guid id = SeedId($"chat:{key}");
			List<Guid> memberIds = members.Select(U).ToList();
			List<UserEntity> users = await db.Set<UserEntity>().AsTracking().Where(x => memberIds.Contains(x.Id)).ToListAsync(_ct);
			DateTime last = now.AddMinutes(-messages[^1].MinutesAgo);
			List<ConversationRead> reads = members.Select(m => new ConversationRead {
				UserId = U(m),
				// The demo player hasn't read the last [unread] messages; everyone else is up to date.
				At = m == "demo" && unread > 0 ? now.AddMinutes(-messages[^(unread + 1)].MinutesAgo) : last
			}).ToList();
			await db.Set<ConversationEntity>().AddAsync(new ConversationEntity {
				Id = id,
				CreatedAt = now.AddMinutes(-messages[0].MinutesAgo - 5),
				CreatorId = U(members[0]),
				Tags = [title == null ? TagConversation.Direct : TagConversation.Group],
				Title = title,
				DirectKey = title == null ? DirectKey(U(members[0]), U(members[1])) : null,
				LastMessageAt = last,
				Users = users,
				JsonData = new ConversationJson { LastMessageText = messages[^1].Text, LastMessageUserId = U(messages[^1].From), Reads = reads }
			}, _ct);
			for (int i = 0; i < messages.Length; i++) {
				(string from, string text, double minutesAgo, string? linkType, Guid? linkId) = messages[i];
				await db.Set<MessageEntity>().AddAsync(new MessageEntity {
					Id = SeedId($"chat:{key}:{i}"),
					CreatedAt = now.AddMinutes(-minutesAgo),
					CreatorId = U(from),
					Tags = [linkType == null ? TagMessage.Text : TagMessage.Shared],
					Text = text,
					ConversationId = id,
					JsonData = new MessageJson { LinkType = linkType, LinkId = linkId }
				}, _ct);
			}

			await db.SaveChangesAsync(_ct);
			db.ChangeTracker.Clear();
		}
	}

	private static readonly Guid padelOpenId = SeedId("tournament:padel-open");

	private static string DirectKey(Guid a, Guid b) => string.CompareOrdinal(a.ToString(), b.ToString()) < 0 ? $"{a}:{b}" : $"{b}:{a}";

	// ---------------- Wrap-up ----------------

	/// <summary>Everything older than a day and a half has been read; the newest stay unread.</summary>
	private async Task FinishNotifications(DateTime now) {
		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
		List<Guid> ids = _userIds;
		DateTime readBefore = now.AddHours(-36);
		foreach (TagNotification kind in Enum.GetValues<TagNotification>().Where(x => (int)x < 200)) {
			List<TagNotification> read = [kind, TagNotification.Read];
			await db.Set<NotificationEntity>()
				.Where(x => ids.Contains(x.UserId) && x.CreatedAt < readBefore && x.Tags.Contains(kind) && x.Tags.Contains(TagNotification.Unread))
				.ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, read), _ct);
		}
	}

	private async Task Count() {
		List<Guid> ids = _userIds;
		_result.Users = ids.Count;
		_result.Venues = await db.Set<VenueEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Courts = await db.Set<CourtEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Bookings = await db.Set<BookingEntity>().CountAsync(x => ids.Contains(x.UserId), _ct);
		_result.Tournaments = await db.Set<TournamentEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.OpenMatches = await db.Set<OpenMatchEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Posts = await db.Set<PostEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Conversations = await db.Set<ConversationEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Messages = await db.Set<MessageEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Notifications = await db.Set<NotificationEntity>().CountAsync(x => ids.Contains(x.UserId), _ct);
	}

	// ---------------- Pictures ----------------
	// Drawn pixel by pixel (ImageSharp has no drawing package here): a court, table or pitch seen from above for venues,
	// a ball for posts and a silhouette for profile photos.

	private static readonly (byte R, byte G, byte B)[] Palette = [
		(30, 90, 200), (220, 80, 60), (40, 150, 110), (150, 70, 200), (230, 150, 30), (20, 140, 170), (200, 60, 130), (90, 110, 130), (60, 60, 160), (170, 120, 60)
	];

	private void Save(string path, Image<Rgb24> image) {
		string full = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "Media", path);
		Directory.CreateDirectory(Path.GetDirectoryName(full)!);
		image.SaveAsJpeg(full, new JpegEncoder { Quality = 82 });
	}

	private static Rgb24 Mix((byte R, byte G, byte B) a, (byte R, byte G, byte B) b, float t) {
		t = Math.Clamp(t, 0, 1);
		return new Rgb24((byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));
	}

	private static Rgb24 Blend(Rgb24 c, (byte R, byte G, byte B) over, float alpha) => Mix((c.R, c.G, c.B), over, alpha);

	private static (byte, byte, byte) Darker((byte R, byte G, byte B) c, float f) => ((byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f));

	private void Draw(string path, int width, int height, Func<int, int, Rgb24> pixel) {
		using Image<Rgb24> image = new(width, height);
		image.ProcessPixelRows(rows => {
			for (int y = 0; y < rows.Height; y++) {
				Span<Rgb24> row = rows.GetRowSpan(y);
				for (int x = 0; x < row.Length; x++) row[x] = pixel(x, y);
			}
		});
		Save(path, image);
	}

	private void DrawAvatar(string path, int index) {
		(byte, byte, byte) a = Palette[index % Palette.Length], b = Palette[(index * 3 + 4) % Palette.Length];
		const int size = 256;
		Draw(path, size, size, (x, y) => {
			Rgb24 c = Mix(a, b, (x + y) / (2f * size));
			float hx = x - size * 0.5f, hy = y - size * 0.4f, sx = (x - size * 0.5f) / (size * 0.36f), sy = (y - size * 0.98f) / (size * 0.34f);
			bool head = hx * hx + hy * hy < size * 0.17f * size * 0.17f, body = sx * sx + sy * sy < 1;
			return head || body ? Blend(c, (255, 255, 255), 0.82f) : c;
		});
	}

	private void DrawVenue(string path, SeedVenue v, int shot) {
		(byte R, byte G, byte B) baseColor = v.Color, surround = Darker(v.Color, 0.45f);
		TagSport? sport = v.Courts.FirstOrDefault()?.Sport;
		const int w = 960, h = 600;
		Random noise = new(v.Key.GetHashCode() ^ shot);
		float[] lights = Enumerable.Range(0, 14).Select(_ => (float)noise.NextDouble()).ToArray();

		// Shot 1 and 2: the place around it (warm light spots); shot 0: the court itself from above.
		if (shot > 0 || sport == null) {
			(byte, byte, byte) warm = shot == 1 ? ((byte)250, (byte)200, (byte)120) : ((byte)255, (byte)240, (byte)210);
			Draw(path, w, h, (x, y) => {
				Rgb24 c = Mix(surround, baseColor, y / (float)h * 0.8f + x / (float)w * 0.2f);
				for (int i = 0; i < lights.Length; i += 2) {
					float dx = x - lights[i] * w, dy = y - lights[i + 1] * h * 0.6f;
					float d = MathF.Sqrt(dx * dx + dy * dy);
					if (d < 140) c = Blend(c, warm, (1 - d / 140) * 0.45f);
				}
				return c;
			});
			return;
		}

		Draw(path, w, h, (x, y) => {
			float mx = 120, my = 70;
			bool inside = x >= mx && x <= w - mx && y >= my && y <= h - my;
			Rgb24 c = inside ? Mix(baseColor, Darker(baseColor, 0.8f), y / (float)h) : Mix(surround, Darker(surround, 0.7f), x / (float)w);
			if (!inside) return c;
			float fx = (x - mx) / (w - 2 * mx), fy = (y - my) / (h - 2 * my);
			float lw = 3f / (w - 2 * mx), lh = 3f / (h - 2 * my);
			bool edge = fx < lw || fx > 1 - lw || fy < lh || fy > 1 - lh;
			bool line = sport switch {
				TagSport.Football => FootballLine(fx, fy, lw, lh),
				TagSport.Billiards or TagSport.Snooker => false,
				TagSport.Squash => MathF.Abs(fy - 0.55f) < lh || fy > 0.55f && MathF.Abs(fx - 0.5f) < lw,
				_ => MathF.Abs(fy - 0.5f) < lh || (MathF.Abs(fy - 0.2f) < lh || MathF.Abs(fy - 0.8f) < lh) || fy > 0.2f && fy < 0.8f && MathF.Abs(fx - 0.5f) < lw
			};
			bool net = sport is TagSport.Padel or TagSport.Tennis && MathF.Abs(fy - 0.5f) < lh * 2.5f;
			if (sport is TagSport.Billiards or TagSport.Snooker) {
				bool cushion = fx < 0.05f || fx > 0.95f || fy < 0.08f || fy > 0.92f;
				float[][] pockets = [[0.03f, 0.05f], [0.5f, 0.03f], [0.97f, 0.05f], [0.03f, 0.95f], [0.5f, 0.97f], [0.97f, 0.95f]];
				bool pocket = pockets.Any(p => MathF.Pow((fx - p[0]) * 1.6f, 2) + MathF.Pow(fy - p[1], 2) < 0.0022f);
				if (pocket) return new Rgb24(15, 15, 15);
				if (cushion) return Mix((110, 60, 25), (80, 40, 15), fy);
			}

			if (net) return new Rgb24(240, 240, 240);
			return edge || line ? Blend(c, (255, 255, 255), 0.9f) : c;
		});
	}

	/// <summary>Halfway line, centre circle and the two penalty boxes of a pitch (fx, fy in 0-1).</summary>
	private static bool FootballLine(float fx, float fy, float lw, float lh) {
		bool halfway = MathF.Abs(fx - 0.5f) < lw;
		float cx = (fx - 0.5f) * 1.6f, cy = fy - 0.5f;
		bool circle = MathF.Abs(MathF.Sqrt(cx * cx + cy * cy) - 0.18f) < lh * 1.5f;
		bool inBoxRows = MathF.Abs(fy - 0.5f) < 0.25f;
		bool boxSide = (MathF.Abs(fx - 0.15f) < lw || MathF.Abs(fx - 0.85f) < lw) && inBoxRows;
		bool boxEdge = (fx < 0.15f || fx > 0.85f) && MathF.Abs(MathF.Abs(fy - 0.5f) - 0.25f) < lh;
		return halfway || circle || boxSide || boxEdge;
	}

	private void DrawPost(string path, string kind, int index) {
		(byte, byte, byte) a = Palette[index % Palette.Length], b = Palette[(index + 5) % Palette.Length];
		const int size = 900;
		(byte, byte, byte) ball = kind switch { "table" => ((byte)230, (byte)40, (byte)40), "pitch" => ((byte)250, (byte)250, (byte)250), "trophy" => ((byte)250, (byte)200, (byte)60), _ => ((byte)220, (byte)240, (byte)60) };
		Draw(path, size, size, (x, y) => {
			Rgb24 c = Mix(a, b, (x * 0.7f + y * 0.3f) / size);
			// Soft stripes, then the ball with its seam.
			if ((x + y) / 60 % 2 == 0) c = Blend(c, (255, 255, 255), 0.05f);
			float dx = x - size * 0.58f, dy = y - size * 0.45f, r = size * 0.24f, d = MathF.Sqrt(dx * dx + dy * dy);
			float sx = x - size * 0.62f, sy = y - size * 0.78f;
			if (d < r) {
				float shade = 1 - d / r * 0.35f;
				c = new Rgb24((byte)(ball.Item1 * shade), (byte)(ball.Item2 * shade), (byte)(ball.Item3 * shade));
				float seam1 = MathF.Abs(MathF.Sqrt((dx + r * 1.15f) * (dx + r * 1.15f) + dy * dy) - r * 0.95f);
				float seam2 = MathF.Abs(MathF.Sqrt((dx - r * 1.15f) * (dx - r * 1.15f) + dy * dy) - r * 0.95f);
				if (kind != "trophy" && (seam1 < 5 || seam2 < 5)) c = Blend(c, (255, 255, 255), 0.85f);
			}
			else if (sx * sx / 4 + sy * sy < 30 * 30) c = Blend(c, (0, 0, 0), 0.12f);
			return c;
		});
	}
}
