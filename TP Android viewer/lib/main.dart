import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'dart:typed_data';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:url_launcher/url_launcher.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations(const [DeviceOrientation.portraitUp, DeviceOrientation.portraitDown]);
  runApp(const PolarisApp());
}

class PolarisColors {
  static const background = Color(0xFF020812);
  static const panel = Color(0xFF071323);
  static const panel2 = Color(0xFF0A192B);
  static const blue = Color(0xFF00B7FF);
  static const blue2 = Color(0xFF168DFF);
  static const purple = Color(0xFF6D35FF);
  static const text = Color(0xFFEAF7FF);
  static const muted = Color(0xFF7FA2BD);
  static const success = Color(0xFF27E69B);
}

class PolarisApp extends StatelessWidget {
  const PolarisApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'TP Viewer',
      theme: ThemeData(
        brightness: Brightness.dark,
        scaffoldBackgroundColor: PolarisColors.background,
        colorScheme: const ColorScheme.dark(primary: PolarisColors.blue, secondary: PolarisColors.purple),
        useMaterial3: true,
      ),
      home: const _PolarisStartupSplash(),
    );
  }
}

class _PolarisStartupSplash extends StatefulWidget {
  const _PolarisStartupSplash();

  @override
  State<_PolarisStartupSplash> createState() => _PolarisStartupSplashState();
}

class _PolarisStartupSplashState extends State<_PolarisStartupSplash> {
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _timer = Timer(const Duration(milliseconds: 1950), () {
      if (!mounted) return;
      Navigator.of(context).pushReplacement(PageRouteBuilder(
        pageBuilder: (_, __, ___) => const PolarisHome(),
        transitionDuration: Duration.zero,
        reverseTransitionDuration: Duration.zero,
      ));
    });
  }

  @override
  void dispose() { _timer?.cancel(); super.dispose(); }

  @override
  Widget build(BuildContext context) => Scaffold(
    backgroundColor: Colors.black,
    body: Center(
      child: SizedBox(
        width: 320, height: 320,
        child: Image.asset(
          'assets/images/TRPOLARIS-VIEWER-CINEMATIC-android.gif',
          width: 320, height: 320, fit: BoxFit.contain,
          cacheWidth: 320, cacheHeight: 320,
          filterQuality: FilterQuality.low, gaplessPlayback: true,
          errorBuilder: (_, __, ___) => Image.asset(
            'assets/images/trpolaris_splash.png',
            width: 320, height: 320, fit: BoxFit.contain, filterQuality: FilterQuality.low,
          ),
        ),
      ),
    ),
  );
}

class PolarisProtocol {
  static const int headerSize = 32;
  static const int maxPayload = 32 * 1024 * 1024;
  static const int kindJpeg = 1;
  static const int kindControl = 2;

  static bool valid(Uint8List h) =>
      h.length >= headerSize && h[0] == 0x50 && h[1] == 0x4a && h[2] == 0x50 && h[3] == 0x39 && h[4] == 9;

  static int u16(Uint8List b, int o) => (b[o] << 8) | b[o + 1];
  static int i32(Uint8List b, int o) => ByteData.sublistView(b, o, o + 4).getInt32(0, Endian.big);
  static int i64(Uint8List b, int o) => ByteData.sublistView(b, o, o + 8).getInt64(0, Endian.big);
}

class PolarisHome extends StatefulWidget {
  const PolarisHome({super.key});

  @override
  State<PolarisHome> createState() => _PolarisHomeState();
}

class _PolarisHomeState extends State<PolarisHome> {
  final _ip = TextEditingController(text: '');
  final _port = TextEditingController(text: '50505');
  final _pin = TextEditingController();
  final _client = PolarisClient();

  int tab = 0;
  String status = 'Sunucu bekleniyor';
  String screenName = '';
  String resolution = '-';
  int fps = 0;
  double mbps = 0;
  int ping = -1;
  bool connecting = false;
  bool autoReconnect = true;
  bool discovering = false;
  String _discoveredName = '';
  int _webPort = 8765;
  String _lastSessionName = '';
  String _lastSessionHost = '';
  int _lastSessionPort = 50505;
  List<PolarisRemoteScreen> _screens = <PolarisRemoteScreen>[];
  String? _selectedScreen;
  String? _confirmedScreen;
  bool _screenChangePending = false;
  bool _loadingScreens = false;
  bool _cursorVisible = false;
  double _cursorX = 0.5;
  double _cursorY = 0.5;

  String _performanceMode = 'Düşük gecikme';
  bool _showStatsOverlay = true;
  bool _keepScreenOn = true;
  String _fitMode = 'Sığdır';
  List<PolarisServerProfile> _serverProfiles = <PolarisServerProfile>[];
  String? _activeProfileId;
  bool _authRetrying = false;

  StreamSubscription? _stateSub;
  StreamSubscription? _controlSub;
  Timer? _pingTimer;
  Timer? _reconnectTimer;
  bool _fullscreenAfterConnect = false;
  bool _openingFullscreen = false;

  @override
  void initState() {
    super.initState();
    // Splash is portrait-only; unlock orientation once the main UI is ready.
    SystemChrome.setPreferredOrientations(const [
      DeviceOrientation.portraitUp,
      DeviceOrientation.portraitDown,
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);
    _initializeStartup();
    _stateSub = _client.states.listen((s) async {
      if (!mounted) return;
      setState(() {
        status = s;
        connecting = s == 'Bağlanıyor...' || s == 'Doğrulanıyor...';
      });
      if (s == 'Bağlandı') {
        _reconnectTimer?.cancel();
        _saveLastSession();
        _loadServerScreens();
        _pingTimer?.cancel();
        _pingTimer = Timer.periodic(const Duration(seconds: 2), (_) => _client.ping());
        if (_fullscreenAfterConnect) {
          _fullscreenAfterConnect = false;
          _openFullscreenWhenReady();
        }
      } else if (s == 'Bağlantı koptu' && autoReconnect) {
        _scheduleReconnect();
      }
    });
    _controlSub = _client.controls.listen((c) async {
      if (!mounted) return;
      final type = c['type']?.toString() ?? '';
      if (type == 'authRequired') {
        if (_pin.text.trim().isNotEmpty && !_authRetrying) {
          // Try the PIN saved in the selected profile once. If it fails, authResult
          // below clears it and opens the PIN dialog so a changed server PIN can be saved.
          _authRetrying = true;
          _client.sendAuth(_pin.text.trim());
        } else {
          final pin = await _askPin();
          if (pin != null) {
            _pin.text = pin;
            _authRetrying = true;
            _client.sendAuth(pin);
          }
        }
      } else if (type == 'authResult') {
        if (c['success'] == true) {
          _authRetrying = false;
          _client.sendHello('Android');
          _client.sendMode('Düşük Gecikme');
          _saveLastSession();
        } else {
          _authRetrying = false;
          _pin.clear();
          final pin = await _askPin();
          if (pin != null) { _pin.text = pin; _authRetrying = true; _client.sendAuth(pin); }
        }
      } else if (type == 'screen') {
        setState(() {
          screenName = c['name']?.toString() ?? c['deviceName']?.toString() ?? '';
          final device = c['deviceName']?.toString() ?? '';
          if (device.isNotEmpty) {
            _selectedScreen = device;
            _confirmedScreen = device;
            _screenChangePending = false;
          }
          final w = c['width'];
          final h = c['height'];
          if (w != null && h != null) resolution = '$w × $h';
        });
        // Fullscreen owns device orientation. Do not change orientation from the
        // background home page, otherwise it fights the live viewer when the
        // Windows display changes between portrait and landscape.
      } else if (type == 'cursor') {
        final x = (c['x'] as num?)?.toDouble();
        final y = (c['y'] as num?)?.toDouble();
        setState(() {
          _cursorVisible = c['visible'] == true && x != null && y != null && x >= 0 && y >= 0;
          if (x != null) _cursorX = x.clamp(0.0, 1.0);
          if (y != null) _cursorY = y.clamp(0.0, 1.0);
        });
      } else if (type == 'telemetry') {
        setState(() {
          fps = (c['fps'] as num?)?.round() ?? fps;
          mbps = (c['mbps'] as num?)?.toDouble() ?? mbps;
        });
      } else if (type == 'pong') {
        final measured = _client.consumePingMs();
        if (measured != null && mounted) setState(() => ping = measured);
      } else if (type == 'screenResult') {
        final success = c['success'] == true;
        final device = c['deviceName']?.toString() ?? '';
        final message = c['message']?.toString() ?? '';
        if (success) {
          setState(() {
            _screenChangePending = false;
            if (device.isNotEmpty) {
              _selectedScreen = device;
              _confirmedScreen = device;
            }
            status = message.isNotEmpty ? message : 'Ekran değiştirildi';
          });
        } else {
          setState(() {
            _screenChangePending = false;
            _selectedScreen = _confirmedScreen;
            status = message.isNotEmpty ? message : 'Ekran değiştirilemedi';
          });
          _showMessage(message.isNotEmpty ? message : 'Bu ekran şu anda kullanılıyor.');
        }
      } else if (type == 'connectionRejected') {
        _showMessage(c['message']?.toString() ?? 'Sunucu bağlantıyı reddetti.');
      }
    });
  }

  Future<void> _initializeStartup() async {
    await _loadLastSession();
    if (!mounted) return;
    await _discoverServer(autoConnect: true, preferredName: _lastSessionName.isNotEmpty ? _lastSessionName : null);
  }

  @override
  void dispose() {
    _pingTimer?.cancel();
    _reconnectTimer?.cancel();
    _stateSub?.cancel();
    _controlSub?.cancel();
    _ip.dispose();
    _port.dispose();
    _pin.dispose();
    _client.dispose();
    super.dispose();
  }

  void _scheduleReconnect() {
    _reconnectTimer?.cancel();
    _reconnectTimer = Timer(const Duration(seconds: 2), () {
      if (mounted && autoReconnect && !_client.connected) _connect();
    });
  }

  Future<void> _discoverServer({bool autoConnect = false, String? preferredName}) async {
    if (discovering || _client.connected) return;
    setState(() {
      discovering = true;
      status = 'Sunucu aranıyor...';
    });
    try {
      final found = await PolarisLanDiscovery.findServer(preferredName: preferredName);
      if (!mounted) return;
      if (found == null) {
        setState(() {
          discovering = false;
          status = 'Sunucu bulunamadı';
        });
        return;
      }
      _ip.text = found.host;
      _port.text = '${found.port}';
      _webPort = found.webPort;
      _discoveredName = found.name;
      final matchIndex = _serverProfiles.indexWhere((p) => p.name.toLowerCase() == found.name.toLowerCase());
      if (matchIndex >= 0) {
        final old = _serverProfiles[matchIndex];
        _serverProfiles[matchIndex] = old.copyWith(host: found.host, port: found.port, lastUsed: DateTime.now().millisecondsSinceEpoch);
        _activeProfileId = old.id;
        _pin.text = old.pin;
        await _saveProfiles();
      }
      setState(() {
        discovering = false;
        status = 'Sunucu bulundu: ${found.name}';
      });
      if (autoConnect && mounted) {
        await _connect();
      }
    } catch (e) {
      if (!mounted) return;
      setState(() {
        discovering = false;
        status = 'Keşif başarısız';
      });
    }
  }

  Future<void> _loadLastSession() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      if (!mounted) return;
      final rawProfiles = prefs.getStringList('server_profiles') ?? <String>[];
      final profiles = <PolarisServerProfile>[];
      for (final raw in rawProfiles) {
        try {
          final obj = jsonDecode(raw);
          if (obj is Map) profiles.add(PolarisServerProfile.fromJson(Map<String, dynamic>.from(obj)));
        } catch (_) {}
      }
      setState(() {
        _serverProfiles = profiles;
        _lastSessionName = prefs.getString('last_session_name') ?? '';
        _lastSessionHost = prefs.getString('last_session_host') ?? '';
        _lastSessionPort = prefs.getInt('last_session_port') ?? 50505;
        autoReconnect = prefs.getBool('auto_reconnect') ?? true;
        _performanceMode = prefs.getString('performance_mode') ?? 'Düşük gecikme';
        _showStatsOverlay = prefs.getBool('show_stats_overlay') ?? true;
        _keepScreenOn = prefs.getBool('keep_screen_on') ?? true;
        _fitMode = prefs.getString('fit_mode') ?? 'Sığdır';
      });
      if (_ip.text.isEmpty && _lastSessionHost.isNotEmpty) {
        _ip.text = _lastSessionHost;
        _port.text = '$_lastSessionPort';
      }
      if (_keepScreenOn) _setKeepScreenOn(true);
    } catch (_) {}
  }

  Future<void> _saveProfiles() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setStringList('server_profiles', _serverProfiles.map((p) => jsonEncode(p.toJson())).toList());
  }

  Future<void> _setPerformanceMode(String value) async {
    setState(() => _performanceMode = value);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('performance_mode', value);
  }

  Future<void> _setShowStatsOverlay(bool value) async {
    setState(() => _showStatsOverlay = value);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('show_stats_overlay', value);
  }

  Future<void> _setKeepScreenOn(bool value) async {
    setState(() => _keepScreenOn = value);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('keep_screen_on', value);
    try { await const MethodChannel('trpolaris/device').invokeMethod('setKeepScreenOn', value); } catch (_) {}
  }

  Future<void> _setFitMode(String value) async {
    setState(() => _fitMode = value);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('fit_mode', value);
  }

  Future<void> _setAutoReconnect(bool value) async {
    setState(() => autoReconnect = value);
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('auto_reconnect', value);
    } catch (_) {}
  }

  Future<void> _openExternalUrl(String url) async {
    try {
      final ok = await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
      if (!ok && mounted) _showMessage('Bağlantı açılamadı.');
    } catch (_) {
      if (mounted) _showMessage('Bağlantı açılamadı.');
    }
  }

  void _showAbout() {
    showModalBottomSheet<void>(
      context: context, backgroundColor: PolarisColors.panel2, isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (sheetContext) => SafeArea(child: Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 20),
        child: SingleChildScrollView(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Center(child: Container(width: 42, height: 4, decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(4)))),
          const SizedBox(height: 20),
          Row(children: [
            Container(width: 58, height: 58, padding: const EdgeInsets.all(7), decoration: BoxDecoration(color: Colors.black, borderRadius: BorderRadius.circular(16), border: Border.all(color: PolarisColors.blue.withOpacity(.45))), child: Image.asset('assets/images/trpolaris_client.png')),
            const SizedBox(width: 14),
            const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('TP Viewer', style: TextStyle(fontSize: 21, fontWeight: FontWeight.w800)),
              SizedBox(height: 4), Text('TRPOLARIS Virtual Display Client', style: TextStyle(color: PolarisColors.muted, fontSize: 11)),
            ])),
          ]),
          const SizedBox(height: 22),
          const Text('UYGULAMA HAKKINDA', style: TextStyle(fontSize: 12, color: PolarisColors.blue, fontWeight: FontWeight.w800, letterSpacing: 1.2)),
          const SizedBox(height: 8),
          const Text('TP Viewer, TRPOLARIS sunucusundaki sanal ekranı Android cihaz üzerinden düşük gecikmeli şekilde görüntülemek için geliştirilmiş istemcidir. Yerel ağ keşfi, PIN doğrulaması, ekran seçimi, gerçek zamanlı görüntü akışı, FPS/bitrate ölçümü ve tam ekran görüntüleme özelliklerini destekler.', style: TextStyle(color: PolarisColors.muted, height: 1.55, fontSize: 13)),
          const SizedBox(height: 18),
          const Text('TEKNİK BİLGİLER', style: TextStyle(fontSize: 12, color: PolarisColors.blue, fontWeight: FontWeight.w800, letterSpacing: 1.2)),
          const SizedBox(height: 6),
          _aboutInfoRow('Sürüm', '1.1.0 (2)'), _aboutInfoRow('Akış protokolü', 'PJP9 / TCP 50505'), _aboutInfoRow('LAN keşfi', 'UDP 50504'), _aboutInfoRow('Görüntü', 'JPEG gerçek zamanlı akış'), _aboutInfoRow('Mod', 'Düşük Gecikme'), _aboutInfoRow('Platform', 'Android'),
          const SizedBox(height: 18),
          const Text('GELİŞTİRİCİ', style: TextStyle(fontSize: 12, color: PolarisColors.blue, fontWeight: FontWeight.w800, letterSpacing: 1.2)),
          const SizedBox(height: 8),
          const Text('TRPOLARIS', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w800)),
          const SizedBox(height: 4),
          const Text('Web geliştirme, yazılım geliştirme ve grafik tasarım odaklı TRPOLARIS projesi.', style: TextStyle(color: PolarisColors.muted, height: 1.45, fontSize: 12)),
          const SizedBox(height: 18),
          SizedBox(width: double.infinity, child: FilledButton(onPressed: () => Navigator.pop(sheetContext), child: const Text('KAPAT'))),
        ])),
      )),
    );
  }

  Widget _aboutInfoRow(String title, String value) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(children: [Expanded(child: Text(title, style: const TextStyle(color: PolarisColors.muted, fontSize: 12))), Text(value, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w700))]),
  );

  Future<void> _saveLastSession() async {
    final host = _ip.text.trim();
    final port = int.tryParse(_port.text.trim()) ?? 50505;
    if (host.isEmpty) return;
    try {
      final prefs = await SharedPreferences.getInstance();
      final name = _discoveredName.isNotEmpty
          ? _discoveredName
          : (screenName.isNotEmpty ? screenName : host);
      await prefs.setString('last_session_name', name);
      await prefs.setString('last_session_host', host);
      await prefs.setInt('last_session_port', port);
      final profileName = _discoveredName.isNotEmpty ? _discoveredName : name;
      final idx = _serverProfiles.indexWhere((p) => p.id == _activeProfileId || p.name.toLowerCase() == profileName.toLowerCase());
      final profile = PolarisServerProfile(
        id: idx >= 0 ? _serverProfiles[idx].id : DateTime.now().microsecondsSinceEpoch.toString(),
        name: profileName, host: host, port: port, pin: _pin.text.trim(), lastUsed: DateTime.now().millisecondsSinceEpoch,
      );
      if (idx >= 0) { _serverProfiles[idx] = profile; } else { _serverProfiles.insert(0, profile); }
      _activeProfileId = profile.id;
      await _saveProfiles();
      if (mounted) {
        setState(() {
          _lastSessionName = name;
          _lastSessionHost = host;
          _lastSessionPort = port;
        });
      }
    } catch (_) {}
  }

  Future<void> _loadServerScreens() async {
    final host = _ip.text.trim();
    if (host.isEmpty || _loadingScreens) return;
    setState(() => _loadingScreens = true);
    try {
      final client = HttpClient();
      client.connectionTimeout = const Duration(seconds: 2);
      final request = await client.getUrl(Uri.parse('http://$host:$_webPort/api/screens'));
      final response = await request.close().timeout(const Duration(seconds: 3));
      final body = await response.transform(utf8.decoder).join();
      client.close(force: true);
      if (response.statusCode != 200) throw HttpException('HTTP ${response.statusCode}');
      final decoded = jsonDecode(body);
      final list = <PolarisRemoteScreen>[];
      if (decoded is List) {
        for (final item in decoded) {
          if (item is! Map) continue;
          final device = item['deviceName']?.toString() ?? '';
          if (device.isEmpty) continue;
          list.add(PolarisRemoteScreen(
            device,
            item['name']?.toString() ?? device,
            int.tryParse('${item['width'] ?? 0}') ?? 0,
            int.tryParse('${item['height'] ?? 0}') ?? 0,
          ));
        }
      }
      if (!mounted) return;
      setState(() {
        _screens = list;
        if (_selectedScreen == null || !_screens.any((x) => x.deviceName == _selectedScreen)) {
          _selectedScreen = _screens.isNotEmpty ? _screens.first.deviceName : null;
        }
        _loadingScreens = false;
      });
      if (_selectedScreen != null && screenName.isEmpty && _screens.isNotEmpty) {
        final current = _screens.firstWhere((x) => x.deviceName == _selectedScreen);
        setState(() => screenName = current.name);
      }
    } catch (_) {
      if (mounted) setState(() => _loadingScreens = false);
    }
  }

  Future<void> _selectScreen(String deviceName) async {
    if (!_client.connected || deviceName.isEmpty || _screenChangePending) return;
    if (deviceName == _confirmedScreen) return;
    setState(() {
      _screenChangePending = true;
      status = 'Ekran değiştiriliyor...';
    });
    _client.sendSelect(deviceName);
  }

  Future<String?> _askPin() async {
    _pin.clear();
    return showDialog<String>(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        backgroundColor: PolarisColors.panel2,
        title: const Text('PIN Gerekli'),
        content: TextField(
          controller: _pin,
          autofocus: true,
          obscureText: true,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'Sunucu PIN', prefixIcon: Icon(Icons.lock_outline)),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('İPTAL')),
          FilledButton(onPressed: () => Navigator.pop(context, _pin.text.trim()), child: const Text('BAĞLAN')),
        ],
      ),
    );
  }

  Future<void> _connectProfile(PolarisServerProfile profile) async {
    if (_client.connected || connecting) return;
    setState(() {
      _activeProfileId = profile.id;
      _ip.text = profile.host;
      _port.text = '${profile.port}';
      _pin.text = profile.pin;
      _discoveredName = profile.name;
      autoReconnect = true;
    });
    // First try LAN discovery by the saved server name. If the server's IP changed,
    // the profile is transparently updated before connecting.
    final found = await PolarisLanDiscovery.findServer(preferredName: profile.name);
    if (found != null && mounted) {
      _ip.text = found.host;
      _port.text = '${found.port}';
      _webPort = found.webPort;
      _discoveredName = found.name;
      final updated = profile.copyWith(host: found.host, port: found.port, lastUsed: DateTime.now().millisecondsSinceEpoch);
      final i = _serverProfiles.indexWhere((p) => p.id == profile.id);
      if (i >= 0) _serverProfiles[i] = updated;
      await _saveProfiles();
    }
    final updated = profile.copyWith(host: _ip.text.trim(), port: int.tryParse(_port.text) ?? profile.port, lastUsed: DateTime.now().millisecondsSinceEpoch);
    final i = _serverProfiles.indexWhere((p) => p.id == profile.id);
    if (i >= 0) _serverProfiles[i] = updated;
    await _saveProfiles();
    await _connect(fullscreenOnConnect: true);
  }

  Future<void> _showServerProfileEditor({PolarisServerProfile? profile}) async {
    final name = TextEditingController(text: profile?.name ?? '');
    final host = TextEditingController(text: profile?.host ?? _ip.text.trim());
    final port = TextEditingController(text: '${profile?.port ?? (int.tryParse(_port.text) ?? 50505)}');
    final pin = TextEditingController(text: profile?.pin ?? '');
    final formKey = GlobalKey<FormState>();
    final result = await showDialog<PolarisServerProfile>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        backgroundColor: PolarisColors.panel2,
        title: Text(profile == null ? 'Sunucu Kaydet' : 'Sunucu Düzenle'),
        content: Form(key: formKey, child: SingleChildScrollView(child: Column(children: [
          TextFormField(controller: name, decoration: const InputDecoration(labelText: 'Sunucu adı'), validator: (v) => v == null || v.trim().isEmpty ? 'Sunucu adı gerekli' : null),
          const SizedBox(height: 10),
          TextFormField(controller: host, decoration: const InputDecoration(labelText: 'IP / Host'), validator: (v) => v == null || v.trim().isEmpty ? 'IP / Host gerekli' : null),
          const SizedBox(height: 10),
          TextFormField(controller: port, keyboardType: TextInputType.number, decoration: const InputDecoration(labelText: 'Port')),
          const SizedBox(height: 10),
          TextFormField(controller: pin, obscureText: true, keyboardType: TextInputType.number, decoration: const InputDecoration(labelText: 'PIN (opsiyonel)', prefixIcon: Icon(Icons.lock_outline))),
        ])),),
        actions: [
          TextButton(onPressed: () => Navigator.pop(dialogContext), child: const Text('İPTAL')),
          FilledButton(onPressed: () {
            if (!(formKey.currentState?.validate() ?? false)) return;
            Navigator.pop(dialogContext, PolarisServerProfile(
              id: profile?.id ?? DateTime.now().microsecondsSinceEpoch.toString(),
              name: name.text.trim(), host: host.text.trim(), port: int.tryParse(port.text.trim()) ?? 50505, pin: pin.text.trim(), lastUsed: profile?.lastUsed ?? 0,
            ));
          }, child: const Text('KAYDET')),
        ],
      ),
    );
    name.dispose(); host.dispose(); port.dispose(); pin.dispose();
    if (result == null || !mounted) return;
    final i = _serverProfiles.indexWhere((p) => p.id == result.id);
    setState(() { if (i >= 0) _serverProfiles[i] = result; else _serverProfiles.insert(0, result); });
    await _saveProfiles();
  }

  Future<void> _deleteServerProfile(PolarisServerProfile profile) async {
    final ok = await showDialog<bool>(context: context, builder: (c) => AlertDialog(backgroundColor: PolarisColors.panel2, title: const Text('Sunucuyu sil?'), content: Text('"${profile.name}" kayıtlı sunuculardan kaldırılacak.'), actions: [TextButton(onPressed: () => Navigator.pop(c, false), child: const Text('İPTAL')), FilledButton(onPressed: () => Navigator.pop(c, true), child: const Text('SİL'))]));
    if (ok != true || !mounted) return;
    setState(() => _serverProfiles.removeWhere((p) => p.id == profile.id));
    await _saveProfiles();
  }

  Future<void> _connect({bool fullscreenOnConnect = false}) async {
    if (_client.connected || connecting) return;

    if (_ip.text.trim().isEmpty) {
      await _discoverServer(autoConnect: false, preferredName: _lastSessionName.isNotEmpty ? _lastSessionName : null);
    }

    final host = _ip.text.trim();
    final port = int.tryParse(_port.text.trim());
    if (host.isEmpty || port == null || port < 1 || port > 65535) {
      _showMessage('Sunucu bulunamadı. IP/host ve port bilgisini kontrol edin.');
      return;
    }
    setState(() {
      connecting = true;
      _fullscreenAfterConnect = fullscreenOnConnect;
    });
    await _client.connect(host, port, initialPin: _pin.text.trim());
  }

  void _disconnect() {
    autoReconnect = false;
    _client.disconnect();
  }

  void _showMessage(String text) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(text), behavior: SnackBarBehavior.floating));
  }

  Future<void> _openFullscreenWhenReady() async {
    if (!mounted || _openingFullscreen || !_client.connected) return;
    for (var i = 0; i < 80; i++) {
      if (!mounted || !_client.connected) return;
      if (_client.latestFrame.value != null) {
        await _openFullscreen();
        return;
      }
      await Future<void>.delayed(const Duration(milliseconds: 50));
    }
    if (mounted && _client.latestFrame.value != null) {
      await _openFullscreen();
    }
  }

  Future<void> _openFullscreen() async {
    if (_openingFullscreen || !mounted) return;
    _openingFullscreen = true;
    final liveFrame = _client.latestFrame.value;
    if (liveFrame == null) {
      _openingFullscreen = false;
      _showMessage('Görüntü henüz hazır değil.');
      return;
    }

    final parts = resolution.split(' × ');
    final w = parts.length == 2 ? double.tryParse(parts[0]) : null;
    final h = parts.length == 2 ? double.tryParse(parts[1]) : null;
    final landscape = w != null && h != null && w > h;

    await SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);
    await SystemChrome.setPreferredOrientations(
      landscape
          ? const [DeviceOrientation.landscapeLeft, DeviceOrientation.landscapeRight]
          : const [DeviceOrientation.portraitUp, DeviceOrientation.portraitDown],
    );

    if (!mounted) return;
    await Navigator.of(context).push(
      PageRouteBuilder(
        opaque: true,
        pageBuilder: (_, __, ___) => PolarisFullscreenPage(
          client: _client,
          initialFrame: liveFrame.jpeg,
          initialWidth: w?.round() ?? 16,
          initialHeight: h?.round() ?? 9,
          fitMode: _fitMode,
          performanceMode: _performanceMode,
          showStatsOverlay: _showStatsOverlay,
        ),
        transitionsBuilder: (_, animation, __, child) => FadeTransition(opacity: animation, child: child),
        transitionDuration: const Duration(milliseconds: 180),
      ),
    );

    _openingFullscreen = false;
    await SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
    await SystemChrome.setPreferredOrientations(const [
      DeviceOrientation.portraitUp,
      DeviceOrientation.portraitDown,
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Stack(children: [
          const _BackgroundGlow(),
          Column(children: [
            _topBar(),
            Expanded(child: tab == 0 ? _homePage() : tab == 1 ? _connectionsPage() : _settingsPage()),
          ]),
        ]),
      ),
      bottomNavigationBar: NavigationBar(
        backgroundColor: const Color(0xFF030A14),
        selectedIndex: tab,
        indicatorColor: PolarisColors.blue.withOpacity(.13),
        onDestinationSelected: (i) => setState(() => tab = i),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.home_outlined), selectedIcon: Icon(Icons.home), label: 'Ana Sayfa'),
          NavigationDestination(icon: Icon(Icons.devices_outlined), selectedIcon: Icon(Icons.devices), label: 'Bağlantılar'),
          NavigationDestination(icon: Icon(Icons.settings_outlined), selectedIcon: Icon(Icons.settings), label: 'Ayarlar'),
        ],
      ),
    );
  }

  Widget _topBar() => Padding(
    padding: const EdgeInsets.fromLTRB(20, 14, 20, 10),
    child: Row(children: [
      Container(
        width: 46, height: 46,
        padding: const EdgeInsets.all(7),
        decoration: BoxDecoration(borderRadius: BorderRadius.circular(14), border: Border.all(color: PolarisColors.blue.withOpacity(.6)), color: PolarisColors.panel),
        child: Image.asset('assets/images/trpolaris_client.png'),
      ),
      const SizedBox(width: 12),
      const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text('TRPOLARIS', style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800, letterSpacing: 1.2)),
        Text('VIRTUAL DISPLAY CLIENT', style: TextStyle(fontSize: 9, color: PolarisColors.muted, letterSpacing: 1.7)),
      ])),
      IconButton(onPressed: () => setState(() => tab = 2), icon: const Icon(Icons.settings_outlined, color: PolarisColors.muted)),
    ]),
  );

  Widget _homePage() => SingleChildScrollView(
    padding: const EdgeInsets.fromLTRB(18, 6, 18, 22),
    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      _hero(),
      const SizedBox(height: 18),
      _connectionCard(),
      const SizedBox(height: 18),
      if (_client.connected) _streamCard(),
      const SizedBox(height: 18),
      const Text('ÖZELLİKLER', style: TextStyle(fontSize: 12, color: PolarisColors.muted, fontWeight: FontWeight.w700, letterSpacing: 1.4)),
      const SizedBox(height: 9),
      _featureRow(),
    ]),
  );

  Widget _hero() => Container(
    width: double.infinity,
    padding: const EdgeInsets.fromLTRB(20, 20, 20, 22),
    decoration: BoxDecoration(
      borderRadius: BorderRadius.circular(22),
      gradient: const LinearGradient(colors: [PolarisColors.panel2, Color(0xFF100C2A)], begin: Alignment.topLeft, end: Alignment.bottomRight),
      border: Border.all(color: PolarisColors.blue.withOpacity(.65)),
    ),
    child: const Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(children: [_StatusDot(), SizedBox(width: 8), Text('VIRTUAL DISPLAY', style: TextStyle(color: PolarisColors.blue, fontSize: 11, fontWeight: FontWeight.w700, letterSpacing: 1.5))]),
      SizedBox(height: 12),
      Text('Ekranını\nher yerde kullan.', style: TextStyle(fontSize: 30, height: 1.05, fontWeight: FontWeight.w800)),
      SizedBox(height: 10),
      Text('TRPOLARIS sunucuna bağlan ve Windows sanal ekranını Android cihazından görüntüle.', style: TextStyle(color: PolarisColors.muted, height: 1.45, fontSize: 13)),
    ]),
  );

  Widget _connectionCard() => _GlassCard(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
    const Row(children: [Icon(Icons.link_rounded, color: PolarisColors.blue, size: 19), SizedBox(width: 8), Text('SUNUCUYA BAĞLAN', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w800, letterSpacing: .8))]),
    const SizedBox(height: 15),
    Row(children: [
      Expanded(child: _input('IP / HOST', _ip)),
      const SizedBox(width: 10),
      SizedBox(width: 112, child: _input('PORT', _port)),
      const SizedBox(width: 6),
      IconButton(
        tooltip: 'Sunucuyu otomatik bul',
        onPressed: discovering || _client.connected ? null : () => _discoverServer(),
        icon: discovering
            ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
            : const Icon(Icons.radar_rounded, color: PolarisColors.blue),
      ),
    ]),
    if (_discoveredName.isNotEmpty) ...[
      const SizedBox(height: 7),
      Align(alignment: Alignment.centerLeft, child: Text('Bulunan: $_discoveredName', style: const TextStyle(color: PolarisColors.muted, fontSize: 10))),
    ],
    const SizedBox(height: 13),
    SizedBox(width: double.infinity, height: 52, child: ElevatedButton.icon(
      onPressed: _client.connected ? _disconnect : (discovering ? null : () => _connect(fullscreenOnConnect: true)),
      icon: Icon(_client.connected ? Icons.link_off : Icons.bolt_rounded),
      label: Text(_client.connected ? 'BAĞLANTIYI KES' : connecting ? 'BAĞLANIYOR...' : 'BAĞLAN'),
      style: ElevatedButton.styleFrom(backgroundColor: _client.connected ? const Color(0xFF9D244D) : PolarisColors.blue2, foregroundColor: Colors.white, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)), textStyle: const TextStyle(fontWeight: FontWeight.w800, letterSpacing: 1)),
    )),
    const SizedBox(height: 12),
    Row(children: [
      Icon(Icons.circle, size: 9, color: _client.connected ? PolarisColors.success : PolarisColors.muted),
      const SizedBox(width: 7),
      Expanded(child: Text(status, style: TextStyle(color: _client.connected ? PolarisColors.success : PolarisColors.muted, fontSize: 11))),
      const Icon(Icons.lock_outline, size: 13, color: PolarisColors.muted), const SizedBox(width: 5), const Text('PJP9 / TCP', style: TextStyle(color: PolarisColors.muted, fontSize: 10)),
    ]),
  ]));

  Widget _streamCard() => _GlassCard(child: Column(children: [
    Row(children: [
      const Icon(Icons.monitor_rounded, color: PolarisColors.blue), const SizedBox(width: 8),
      Expanded(child: _screens.isEmpty
          ? Text(screenName.isEmpty ? 'TRPOLARIS DISPLAY' : screenName, style: const TextStyle(fontWeight: FontWeight.w800))
          : DropdownButtonHideUnderline(child: DropdownButton<String>(
        value: _screens.any((x) => x.deviceName == (_selectedScreen ?? _confirmedScreen)) ? (_selectedScreen ?? _confirmedScreen) : null,
        isExpanded: true,
        dropdownColor: PolarisColors.panel2,
        icon: const Icon(Icons.keyboard_arrow_down_rounded, color: PolarisColors.blue),
        items: _screens.map((x) => DropdownMenuItem<String>(value: x.deviceName, child: Text('${x.name} • ${x.width}×${x.height}', maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w700)))).toList(),
        onChanged: _screenChangePending ? null : (value) { if (value != null) _selectScreen(value); },
      ))),
      if (_loadingScreens) const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2)),
      IconButton(onPressed: _openFullscreen, icon: const Icon(Icons.fullscreen)),
    ]),
    const SizedBox(height: 10),
    AspectRatio(
      aspectRatio: _frameAspectRatio(),
      child: ClipRRect(borderRadius: BorderRadius.circular(12), child: Container(
        color: Colors.black,
        alignment: Alignment.center,
        child: ValueListenableBuilder<PolarisFrame?>(
          valueListenable: _client.latestFrame,
          builder: (context, latest, _) {
            if (latest == null) return const Column(mainAxisAlignment: MainAxisAlignment.center, children: [CircularProgressIndicator(), SizedBox(height: 10), Text('İlk görüntü bekleniyor...', style: TextStyle(color: PolarisColors.muted))]);
            return LayoutBuilder(builder: (context, constraints) {
              final boxW = constraints.maxWidth;
              final boxH = constraints.maxHeight;
              final aspect = latest.height > 0 ? latest.width / latest.height : 16 / 9;
              var imageW = boxW;
              var imageH = boxW / aspect;
              if (imageH > boxH) { imageH = boxH; imageW = boxH * aspect; }
              final left = (boxW - imageW) / 2;
              final top = (boxH - imageH) / 2;
              return Stack(children: [
                Positioned.fill(child: RepaintBoundary(child: _LowLatencyJpeg(frame: latest.jpeg, fit: _fitMode == 'Doldur' ? BoxFit.cover : BoxFit.contain, targetWidth: _decodeTargetWidth(boxW, context)))),
                if (_cursorVisible) Positioned(
                  left: (left + _cursorX * imageW).clamp(0.0, boxW - 1),
                  top: (top + _cursorY * imageH).clamp(0.0, boxH - 1),
                  child: const IgnorePointer(child: _PolarisMouseCursor()),
                ),
              ]);
            });
          },
        ),
      )),
    ),
    const SizedBox(height: 10),
    ValueListenableBuilder<PolarisStreamStats>(
      valueListenable: _client.stats,
      builder: (context, st, _) => Row(children: [
        _stat('FPS', '${st.fps}'),
        _stat('BITRATE', '${st.mbps.toStringAsFixed(1)} Mbps'),
        _stat('PING', ping < 0 ? '-' : '$ping ms'),
        _stat('ÇÖZÜNÜRLÜK', _client.latestFrame.value == null ? resolution : '${_client.latestFrame.value!.width} × ${_client.latestFrame.value!.height}'),
      ]),
    ),
  ]));

  int _decodeTargetWidth(double logicalWidth, BuildContext context, {bool fullscreen = false}) {
    final dpr = MediaQuery.devicePixelRatioOf(context);
    final px = (logicalWidth * dpr).round();
    if (_performanceMode == 'Maksimum FPS') return px.clamp(360, fullscreen ? 960 : 840);
    if (_performanceMode == 'Dengeli') return px.clamp(360, fullscreen ? 1080 : 960);
    return px.clamp(360, fullscreen ? 1080 : 960);
  }

  double _frameAspectRatio() {
    final parts = resolution.split(' × ');
    if (parts.length == 2) {
      final w = double.tryParse(parts[0]);
      final h = double.tryParse(parts[1]);
      if (w != null && h != null && h > 0) return w / h;
    }
    return 16 / 9;
  }

  Widget _stat(String title, String value) => Expanded(child: Column(children: [Text(value, maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w800, color: PolarisColors.text)), const SizedBox(height: 3), Text(title, style: const TextStyle(fontSize: 7, color: PolarisColors.muted, letterSpacing: .5))]));

  Widget _input(String label, TextEditingController controller) => TextField(controller: controller, style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600), keyboardType: label == 'PORT' ? TextInputType.number : TextInputType.text, decoration: InputDecoration(labelText: label, labelStyle: const TextStyle(fontSize: 9, color: PolarisColors.muted, letterSpacing: 1), filled: true, fillColor: PolarisColors.background.withOpacity(.65), contentPadding: const EdgeInsets.symmetric(horizontal: 13, vertical: 14), border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide(color: PolarisColors.blue.withOpacity(.22)))));

  Widget _connectionsPage() => ListView(padding: const EdgeInsets.all(18), children: [
    const Text('BAĞLANTILAR', style: TextStyle(fontSize: 24, fontWeight: FontWeight.w800)),
    const SizedBox(height: 6), const Text('Son bağlanan ve mevcut sunucular', style: TextStyle(color: PolarisColors.muted)),
    const SizedBox(height: 18),
    if (_lastSessionHost.isNotEmpty) ...[
      _serverCard(_lastSessionName.isEmpty ? 'SON OTURUM' : _lastSessionName, '$_lastSessionHost:$_lastSessionPort', _client.connected && _ip.text.trim() == _lastSessionHost),
      const SizedBox(height: 4),
    ],
    if (_ip.text.trim().isNotEmpty && _ip.text.trim() != _lastSessionHost)
      _serverCard(_discoveredName.isEmpty ? 'MEVCUT SUNUCU' : _discoveredName, '${_ip.text.trim()}:${_port.text.trim()}', _client.connected),
    if (_lastSessionHost.isEmpty && _ip.text.trim().isEmpty)
      const Padding(padding: EdgeInsets.all(20), child: Center(child: Text('Henüz kayıtlı bağlantı yok.', style: TextStyle(color: PolarisColors.muted)))),
  ]);

  Widget _serverCard(String name, String address, bool online) {
    final parts = address.split(':');
    final host = parts.isNotEmpty ? parts.first : '';
    final port = parts.length > 1 ? parts.last : '50505';
    return Container(margin: const EdgeInsets.only(bottom: 9), padding: const EdgeInsets.all(14), decoration: BoxDecoration(color: PolarisColors.panel.withOpacity(.88), borderRadius: BorderRadius.circular(15), border: Border.all(color: Colors.white.withOpacity(.06))), child: Row(children: [
      Container(width: 42, height: 42, decoration: BoxDecoration(borderRadius: BorderRadius.circular(12), color: PolarisColors.background, border: Border.all(color: PolarisColors.blue.withOpacity(.18))), child: const Icon(Icons.dns_outlined, color: PolarisColors.blue, size: 20)),
      const SizedBox(width: 12), Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(name, style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w700)), const SizedBox(height: 4), Text(address, style: const TextStyle(color: PolarisColors.muted, fontSize: 11))])),
      if (online) const Text('BAĞLI', style: TextStyle(color: PolarisColors.success, fontSize: 8, fontWeight: FontWeight.w800))
      else FilledButton.tonalIcon(
        onPressed: () {
          _ip.text = host;
          _port.text = port;
          autoReconnect = true;
          setState(() {});
          _connect(fullscreenOnConnect: true);
        },
        icon: const Icon(Icons.link, size: 16),
        label: const Text('BAĞLAN', style: TextStyle(fontSize: 9, fontWeight: FontWeight.w800)),
      ),
    ]));
  }

  Widget _settingsPage() => ListView(padding: const EdgeInsets.fromLTRB(18, 18, 18, 28), children: [
    const Text('AYARLAR', style: TextStyle(fontSize: 24, fontWeight: FontWeight.w800)),
    const SizedBox(height: 6), const Text('TP Viewer', style: TextStyle(color: PolarisColors.muted)), const SizedBox(height: 20),
    _settingsSection([
      SwitchListTile(value: autoReconnect, onChanged: _setAutoReconnect, title: const Text('Otomatik yeniden bağlan'), subtitle: const Text('Bağlantı kesildiğinde son sunucuya tekrar bağlan', style: TextStyle(color: PolarisColors.muted)), secondary: const Icon(Icons.refresh_rounded, color: PolarisColors.blue)),
      const Divider(height: 1, color: Colors.white10),
      SwitchListTile(value: _keepScreenOn, onChanged: _setKeepScreenOn, title: const Text('Ekranı açık tut'), subtitle: const Text('Görüntü izlerken ekranın kapanmasını engelle', style: TextStyle(color: PolarisColors.muted)), secondary: const Icon(Icons.brightness_high_outlined, color: PolarisColors.blue)),
      const Divider(height: 1, color: Colors.white10),
      SwitchListTile(value: _showStatsOverlay, onChanged: _setShowStatsOverlay, title: const Text('Fullscreen istatistikleri'), subtitle: const Text('FPS, Mbps ve çözünürlüğü fullscreen üzerinde göster', style: TextStyle(color: PolarisColors.muted)), secondary: const Icon(Icons.analytics_outlined, color: PolarisColors.blue)),
      const Divider(height: 1, color: Colors.white10),
      ListTile(leading: const Icon(Icons.speed_rounded, color: PolarisColors.blue), title: const Text('Performans modu'), subtitle: Text(_performanceMode, style: const TextStyle(color: PolarisColors.muted, fontSize: 11)), trailing: const Icon(Icons.chevron_right_rounded, color: PolarisColors.muted), onTap: _showPerformancePicker),
      const Divider(height: 1, color: Colors.white10),
      ListTile(leading: const Icon(Icons.fit_screen_outlined, color: PolarisColors.blue), title: const Text('Görüntüleme'), subtitle: Text(_fitMode, style: const TextStyle(color: PolarisColors.muted, fontSize: 11)), trailing: const Icon(Icons.chevron_right_rounded, color: PolarisColors.muted), onTap: _showFitPicker),
    ]),
    const SizedBox(height: 22),
    Row(children: [const Expanded(child: Text('KAYITLI SUNUCULAR', style: TextStyle(fontSize: 12, color: PolarisColors.muted, fontWeight: FontWeight.w700, letterSpacing: 1.4))), IconButton(onPressed: () => _showServerProfileEditor(), icon: const Icon(Icons.add_circle_outline, color: PolarisColors.blue))]),
    const SizedBox(height: 8),
    if (_serverProfiles.isEmpty)
      Container(padding: const EdgeInsets.all(18), decoration: BoxDecoration(color: PolarisColors.panel.withOpacity(.86), borderRadius: BorderRadius.circular(16), border: Border.all(color: Colors.white.withOpacity(.06))), child: const Text('Kayıtlı sunucu yok. IP ve PIN bilgisini kaydederek sonraki bağlantılarda tek dokunuşla kullanabilirsin. IP değişirse otomatik keşif ile profil güncellenir.', style: TextStyle(color: PolarisColors.muted, height: 1.45, fontSize: 12)))
    else
      ..._serverProfiles.map(_profileCard),
    const SizedBox(height: 22),
    _settingsSection([
      ValueListenableBuilder<PolarisStreamStats>(valueListenable: _client.stats, builder: (context, st, _) => ListTile(leading: Icon(st.active ? Icons.wifi_tethering_rounded : Icons.wifi_off_rounded, color: st.active ? PolarisColors.success : PolarisColors.muted), title: const Text('Akış durumu'), subtitle: Text(st.active ? 'Canlı veri alınıyor • ${st.ageMs} ms önce frame' : (_client.connected ? 'Bağlı, görüntü bekleniyor' : 'Şu anda bir sunucuya bağlı değil'), style: const TextStyle(color: PolarisColors.muted, fontSize: 11)), trailing: Text(st.active ? '${st.fps} FPS\n${st.mbps.toStringAsFixed(1)} Mbps' : '—', textAlign: TextAlign.right, style: const TextStyle(color: PolarisColors.text, fontSize: 11, fontWeight: FontWeight.w700)))),
      const Divider(height: 1, color: Colors.white10),
      ListTile(leading: const Icon(Icons.aspect_ratio_outlined, color: PolarisColors.blue), title: const Text('Çözünürlük'), subtitle: const Text('Aktif görüntü çözünürlüğü', style: TextStyle(color: PolarisColors.muted, fontSize: 11)), trailing: Text(resolution, style: const TextStyle(color: PolarisColors.muted, fontSize: 11))),
      const Divider(height: 1, color: Colors.white10),
      ListTile(leading: const Icon(Icons.info_outline, color: PolarisColors.blue), title: const Text('Hakkında'), subtitle: const Text('TP Viewer ve TRPOLARIS hakkında detaylı bilgi', style: TextStyle(color: PolarisColors.muted, fontSize: 11)), trailing: const Icon(Icons.chevron_right_rounded, color: PolarisColors.muted), onTap: _showAbout),
    ]),
    const SizedBox(height: 24),
    const Text('SOSYAL MEDYA', style: TextStyle(fontSize: 12, color: PolarisColors.muted, fontWeight: FontWeight.w700, letterSpacing: 1.4)),
    const SizedBox(height: 10),
    Container(width: double.infinity, padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 14), decoration: BoxDecoration(color: PolarisColors.panel.withOpacity(.86), borderRadius: BorderRadius.circular(18), border: Border.all(color: Colors.white.withOpacity(.06))), child: Row(mainAxisAlignment: MainAxisAlignment.spaceAround, children: [
      _socialButton('facebook48_48.png', 'Facebook', 'https://www.facebook.com/celill.ylmz'), _socialButton('instagram48_48.png', 'Instagram', 'https://www.instagram.com/celill.ylmz/'), _socialButton('twitter48_48.png', 'X', 'https://x.com/celill_ylmz'), _socialButton('github48_48.png', 'GitHub', 'https://github.com/trpolaris'), _socialButton('youtube48_48.png', 'YouTube', 'https://www.youtube.com/@trpolaris'),
    ])),
    const SizedBox(height: 12), Center(child: Text('TP Viewer • TRPOLARIS • v1.1.0', style: const TextStyle(color: PolarisColors.muted, fontSize: 10))),
  ]);

  Widget _settingsSection(List<Widget> children) => Container(decoration: BoxDecoration(color: PolarisColors.panel.withOpacity(.86), borderRadius: BorderRadius.circular(16), border: Border.all(color: Colors.white.withOpacity(.06))), child: Column(children: children));

  Widget _profileCard(PolarisServerProfile profile) => Container(margin: const EdgeInsets.only(bottom: 9), padding: const EdgeInsets.all(14), decoration: BoxDecoration(color: PolarisColors.panel.withOpacity(.88), borderRadius: BorderRadius.circular(15), border: Border.all(color: profile.id == _activeProfileId ? PolarisColors.blue.withOpacity(.45) : Colors.white.withOpacity(.06))), child: Row(children: [
    Container(width: 42, height: 42, decoration: BoxDecoration(borderRadius: BorderRadius.circular(12), color: PolarisColors.background), child: const Icon(Icons.dns_outlined, color: PolarisColors.blue, size: 20)),
    const SizedBox(width: 12), Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(profile.name, style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w700)), const SizedBox(height: 3), Text('${profile.host}:${profile.port}', style: const TextStyle(color: PolarisColors.muted, fontSize: 11)), const SizedBox(height: 3), Text(profile.pin.isEmpty ? 'PIN kayıtlı değil' : 'PIN kayıtlı • otomatik giriş', style: const TextStyle(color: PolarisColors.muted, fontSize: 9))])),
    IconButton(tooltip: 'Düzenle', onPressed: () => _showServerProfileEditor(profile: profile), icon: const Icon(Icons.edit_outlined, size: 18, color: PolarisColors.muted)),
    IconButton(tooltip: 'Sil', onPressed: () => _deleteServerProfile(profile), icon: const Icon(Icons.delete_outline, size: 18, color: Colors.redAccent)),
    FilledButton(onPressed: _client.connected || connecting ? null : () => _connectProfile(profile), child: const Text('BAĞLAN', style: TextStyle(fontSize: 9))),
  ]));

  Future<void> _showPerformancePicker() async {
    final value = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: PolarisColors.panel2,
      builder: (c) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: ['Düşük gecikme', 'Dengeli', 'Maksimum FPS'].map((x) {
              final subtitle = x == 'Maksimum FPS'
                  ? 'Decode yükünü azaltır, akıcılığı önceler.'
                  : x == 'Dengeli'
                      ? 'Kalite ve performans dengesi.'
                      : 'En düşük görüntü gecikmesi.';
              return RadioListTile<String>(
                value: x,
                groupValue: _performanceMode,
                onChanged: (v) => Navigator.pop(c, v),
                title: Text(x),
                subtitle: Text(subtitle),
              );
            }).toList(),
          ),
        );
      },
    );
    if (value != null) await _setPerformanceMode(value);
  }

  Future<void> _showFitPicker() async {
    final value = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: PolarisColors.panel2,
      builder: (c) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: ['Sığdır', 'Doldur'].map((x) {
              return RadioListTile<String>(
                value: x,
                groupValue: _fitMode,
                onChanged: (v) => Navigator.pop(c, v),
                title: Text(x),
              );
            }).toList(),
          ),
        );
      },
    );
    if (value != null) await _setFitMode(value);
  }

  Widget _socialButton(String asset, String label, String url) => InkWell(borderRadius: BorderRadius.circular(14), onTap: () => _openExternalUrl(url), child: Padding(padding: const EdgeInsets.all(5), child: Column(mainAxisSize: MainAxisSize.min, children: [SizedBox(width: 42, height: 42, child: Image.asset('assets/images/$asset', fit: BoxFit.contain, filterQuality: FilterQuality.high)), const SizedBox(height: 4), Text(label, style: const TextStyle(color: PolarisColors.muted, fontSize: 8))])));

  Widget _featureRow() => Row(children: const [Expanded(child: _MiniFeature(icon: Icons.speed_rounded, title: 'DÜŞÜK GECİKME')), SizedBox(width: 8), Expanded(child: _MiniFeature(icon: Icons.high_quality_outlined, title: '60 FPS')), SizedBox(width: 8), Expanded(child: _MiniFeature(icon: Icons.screen_rotation_alt_outlined, title: 'PORTRAIT'))]);
}

class PolarisClient {
  Socket? _socket;
  StreamSubscription<List<int>>? _socketSub;
  final _states = StreamController<String>.broadcast();
  final _controls = StreamController<Map<String, dynamic>>.broadcast();
  final ValueNotifier<PolarisFrame?> latestFrame = ValueNotifier<PolarisFrame?>(null);
  final ValueNotifier<PolarisStreamStats> stats = ValueNotifier<PolarisStreamStats>(const PolarisStreamStats());
  Timer? _statsTimer;
  Stopwatch? _statsWatch;
  int _statsFrames = 0;
  int _statsBytes = 0;
  DateTime? _lastFrameAt;
  // Growable receive buffer. Unlike BytesBuilder.toBytes()+sublist(), this
  // avoids copying the whole TCP backlog for every packet.
  Uint8List _rx = Uint8List(64 * 1024);
  int _rxStart = 0;
  int _rxEnd = 0;

  bool _processing = false;
  bool _authSent = false;
  String _host = '';
  int _port = 0;
  int _lastPingTicks = 0;

  void _appendData(List<int> data) {
    if (data.isEmpty) return;
    final needed = _rxEnd + data.length;
    if (needed > _rx.length) {
      _compactRx();
      if (_rxEnd + data.length > _rx.length) {
        var cap = _rx.length;
        while (cap < _rxEnd + data.length) {
          cap *= 2;
        }
        final next = Uint8List(cap);
        next.setRange(0, _rxEnd, _rx);
        _rx = next;
      }
    }
    _rx.setRange(_rxEnd, _rxEnd + data.length, data);
    _rxEnd += data.length;
  }

  int get _rxLength => _rxEnd - _rxStart;

  void _compactRx() {
    if (_rxStart == 0) return;
    final len = _rxEnd - _rxStart;
    if (len > 0) {
      _rx.setRange(0, len, _rx, _rxStart);
    }
    _rxStart = 0;
    _rxEnd = len;
  }

  void _clearRx() {
    _rxStart = 0;
    _rxEnd = 0;
  }

  void _consumeRx(int count) {
    _rxStart += count;
    if (_rxStart == _rxEnd) {
      _clearRx();
    } else if (_rxStart > _rx.length ~/ 2) {
      _compactRx();
    }
  }

  Stream<String> get states => _states.stream;
  Stream<Map<String, dynamic>> get controls => _controls.stream;
  bool get connected => _socket != null;

  Future<void> connect(String host, int port, {String initialPin = ''}) async {
    disconnect(notify: false);
    _host = host; _port = port;
    _resetStats();
    _states.add('Bağlanıyor...');
    try {
      final s = await Socket.connect(host, port, timeout: const Duration(seconds: 6));
      s.setOption(SocketOption.tcpNoDelay, true);
      _socket = s;
      _states.add('Doğrulanıyor...');
      _socketSub = s.listen((data) {
        _appendData(data);
        _drain();
      }, onError: (_) => _lost(), onDone: _lost, cancelOnError: true);
      _statsWatch = Stopwatch()..start();
      _statsTimer?.cancel();
      _statsTimer = Timer.periodic(const Duration(milliseconds: 500), (_) => _publishStats());
      sendAuth(initialPin);
    } catch (_) {
      _socket = null;
      _states.add('Bağlantı koptu');
    }
  }

  void sendAuth(String pin) {
    if (_socket == null) return;
    _authSent = true;
    _write('POLARIS_AUTH|$pin\n');
  }

  void sendHello(String name) => _write('POLARIS_HELLO|$name\n');
  void sendMode(String mode) => _write('POLARIS_MODE|${Uri.encodeComponent(mode)}\n');
  void sendSelect(String deviceName) => _write('POLARIS_SELECT|$deviceName\n');

  void ping() {
    if (_socket == null) return;
    _lastPingTicks = _stopwatchTicks();
    _write('POLARIS_PING|$_lastPingTicks\n');
  }

  int? consumePingMs() {
    if (_lastPingTicks <= 0) return null;
    final elapsedMicros = _stopwatchTicks() - _lastPingTicks;
    _lastPingTicks = 0;
    if (elapsedMicros < 0) return null;
    return (elapsedMicros / 1000).round();
  }

  void setPingFromStopwatchTicks(int sent) {
    final elapsed = _stopwatchTicks() - sent;
    if (elapsed >= 0) {
      // Kept for compatibility with older callers.
    }
  }

  int _stopwatchTicks() => DateTime.now().microsecondsSinceEpoch;

  void _write(String text) {
    try { _socket?.add(utf8.encode(text)); } catch (_) {}
  }

  void _drain() {
    if (_processing) return;
    _processing = true;
    try {
      while (true) {
        if (_rxLength < PolarisProtocol.headerSize) break;

        final headerOffset = _rxStart;
        if (_rx[headerOffset] != 0x50 ||
            _rx[headerOffset + 1] != 0x4a ||
            _rx[headerOffset + 2] != 0x50 ||
            _rx[headerOffset + 3] != 0x39 ||
            _rx[headerOffset + 4] != 9) {
          var index = -1;
          final end = _rxEnd - 3;
          for (var i = _rxStart + 1; i < end; i++) {
            if (_rx[i] == 0x50 &&
                _rx[i + 1] == 0x4a &&
                _rx[i + 2] == 0x50 &&
                _rx[i + 3] == 0x39) {
              index = i;
              break;
            }
          }
          if (index < 0) {
            // Preserve only a possible partial "PJP9" marker.
            final keep = _rxLength > 3 ? 3 : _rxLength;
            if (keep > 0) {
              _rx.setRange(0, keep, _rx, _rxEnd - keep);
            }
            _rxStart = 0;
            _rxEnd = keep;
          } else {
            _rxStart = index;
          }
          continue;
        }

        final header = Uint8List(PolarisProtocol.headerSize);
        header.setRange(0, PolarisProtocol.headerSize, _rx, _rxStart);

        final kind = header[5];
        final payloadLength = PolarisProtocol.i32(header, 28);
        if (payloadLength <= 0 || payloadLength > PolarisProtocol.maxPayload) {
          _lost();
          break;
        }

        final packetLength = PolarisProtocol.headerSize + payloadLength;
        if (_rxLength < packetLength) break;

        final payload = Uint8List(payloadLength);
        payload.setRange(0, payloadLength, _rx, _rxStart + PolarisProtocol.headerSize);
        _consumeRx(packetLength);

        if (kind == PolarisProtocol.kindControl) {
          try {
            final obj = jsonDecode(utf8.decode(payload));
            if (obj is Map) {
              final map = Map<String, dynamic>.from(obj);
              _controls.add(map);
              if (map['type'] == 'authResult' && map['success'] == true) {
                _states.add('Bağlandı');
              }
            }
          } catch (_) {}
        } else if (kind == PolarisProtocol.kindJpeg) {
          final width = PolarisProtocol.i32(header, 8);
          final height = PolarisProtocol.i32(header, 12);
          final fpsValue = PolarisProtocol.i32(header, 16);
          _statsFrames++;
          _statsBytes += packetLength;
          _lastFrameAt = DateTime.now();
          final frame = PolarisFrame(payload, width, height, fpsValue);
          latestFrame.value = frame;
        }
      }
    } finally {
      _processing = false;
    }
  }

  void _resetStats() {
    _statsTimer?.cancel();
    _statsTimer = null;
    _statsWatch = null;
    _statsFrames = 0;
    _statsBytes = 0;
    _lastFrameAt = null;
    stats.value = const PolarisStreamStats();
  }

  void _publishStats() {
    final watch = _statsWatch;
    if (watch == null || !watch.isRunning) return;
    final elapsed = watch.elapsedMicroseconds / 1000000.0;
    if (elapsed <= 0) return;
    final age = _lastFrameAt == null ? 999999 : DateTime.now().difference(_lastFrameAt!).inMilliseconds;
    stats.value = PolarisStreamStats(
      fps: (_statsFrames / elapsed).round(),
      mbps: (_statsBytes * 8.0) / elapsed / 1000000.0,
      active: _statsFrames > 0 && age < 1500,
      ageMs: age.clamp(0, 999999),
    );
    _statsFrames = 0;
    _statsBytes = 0;
    _statsWatch = Stopwatch()..start();
  }

  void _lost() {
    if (_socket == null) return;
    _socketSub?.cancel(); _socketSub = null;
    try { _socket?.destroy(); } catch (_) {}
    _socket = null;
    _resetStats();
    _states.add('Bağlantı koptu');
  }

  void disconnect({bool notify = true}) {
    _socketSub?.cancel(); _socketSub = null;
    try { _socket?.destroy(); } catch (_) {}
    _socket = null;
    _clearRx();
    _resetStats();
    if (notify) _states.add('Bağlantı koptu');
  }

  void dispose() {
    disconnect(notify: false);
    latestFrame.dispose();
    stats.dispose();
    _states.close(); _controls.close();
  }
}

class PolarisStreamStats {
  final int fps;
  final double mbps;
  final bool active;
  final int ageMs;
  const PolarisStreamStats({this.fps = 0, this.mbps = 0, this.active = false, this.ageMs = 0});
}

class PolarisFrame {
  final Uint8List jpeg;
  final int width;
  final int height;
  final int fps;
  PolarisFrame(this.jpeg, this.width, this.height, this.fps);
}

class PolarisServerProfile {
  final String id;
  final String name;
  final String host;
  final int port;
  final String pin;
  final int lastUsed;
  const PolarisServerProfile({required this.id, required this.name, required this.host, required this.port, required this.pin, required this.lastUsed});

  PolarisServerProfile copyWith({String? host, int? port, String? pin, int? lastUsed, String? name}) => PolarisServerProfile(id: id, name: name ?? this.name, host: host ?? this.host, port: port ?? this.port, pin: pin ?? this.pin, lastUsed: lastUsed ?? this.lastUsed);
  Map<String, dynamic> toJson() => {'id': id, 'name': name, 'host': host, 'port': port, 'pin': pin, 'lastUsed': lastUsed};
  factory PolarisServerProfile.fromJson(Map<String, dynamic> j) => PolarisServerProfile(id: j['id']?.toString() ?? DateTime.now().microsecondsSinceEpoch.toString(), name: j['name']?.toString() ?? 'Sunucu', host: j['host']?.toString() ?? '', port: int.tryParse('${j['port'] ?? 50505}') ?? 50505, pin: j['pin']?.toString() ?? '', lastUsed: int.tryParse('${j['lastUsed'] ?? 0}') ?? 0);
}

class DiscoveredServer {
  final String host;
  final int port;
  final String name;
  final int webPort;
  const DiscoveredServer(this.host, this.port, this.name, this.webPort);
}

class PolarisRemoteScreen {
  final String deviceName;
  final String name;
  final int width;
  final int height;
  const PolarisRemoteScreen(this.deviceName, this.name, this.width, this.height);
}

class PolarisLanDiscovery {
  static const int primaryPort = 50504;
  static const int legacyPort = 50505;
  static const String request = 'POLARIS_DISCOVER|1';
  static const MethodChannel _wifiChannel = MethodChannel('trpolaris/wifi');

  static Future<List<String>> _broadcastAddresses() async {
    final result = <String>{'255.255.255.255'};
    try {
      final raw = await _wifiChannel.invokeMethod<List<dynamic>>('wifiBroadcasts');
      if (raw != null) {
        for (final item in raw) {
          final value = item?.toString().trim();
          if (value != null && value.isNotEmpty) result.add(value);
        }
      }
    } catch (_) {}
    return result.toList();
  }

  static Future<DiscoveredServer?> findServer({String? preferredName}) async {
    // Windows Server discovery is UDP 50504; TCP display is 50505.
    // If a saved profile has a name, try that server first so an IP change is transparent.
    final broadcasts = await _broadcastAddresses();
    if (preferredName != null && preferredName.trim().isNotEmpty) {
      for (final port in <int>[primaryPort, legacyPort]) {
        for (var attempt = 0; attempt < 2; attempt++) {
          final found = await _probe(port, broadcasts, preferredName: preferredName);
          if (found != null) return found;
        }
      }
    }
    // Fall back to normal discovery so a new/different server is still found.
    for (final port in <int>[primaryPort, legacyPort]) {
      for (var attempt = 0; attempt < 2; attempt++) {
        final found = await _probe(port, broadcasts);
        if (found != null) return found;
      }
    }
    return null;
  }

  static Future<DiscoveredServer?> _probe(int discoveryPort, List<String> broadcasts, {String? preferredName}) async {
    RawDatagramSocket? socket;
    StreamSubscription<RawSocketEvent>? sub;
    try {
      socket = await RawDatagramSocket.bind(InternetAddress.anyIPv4, 0);
      socket.broadcastEnabled = true;
      final data = Uint8List.fromList(utf8.encode(request));
      final completer = Completer<DiscoveredServer?>();
      sub = socket.listen((event) {
        if (event != RawSocketEvent.read) return;
        final dg = socket?.receive();
        if (dg == null) return;
        try {
          final text = utf8.decode(dg.data, allowMalformed: true);
          final obj = jsonDecode(text);
          if (obj is Map && obj['type']?.toString() == 'polaris-server') {
            final p = int.tryParse('${obj['port'] ?? 50505}') ?? 50505;
            final name = obj['name']?.toString() ?? 'POLARIS-SERVER';
            final webPort = int.tryParse('${obj['webPort'] ?? 8765}') ?? 8765;
            final server = DiscoveredServer(dg.address.address, p, name, webPort);
            if (preferredName == null || preferredName.trim().isEmpty || name.toLowerCase() == preferredName.trim().toLowerCase()) {
              if (!completer.isCompleted) completer.complete(server);
            }
          }
        } catch (_) {}
      });
      for (final address in broadcasts) {
        try {
          socket.send(data, InternetAddress(address), discoveryPort);
        } catch (_) {}
      }
      final result = await Future.any<DiscoveredServer?>([
        completer.future,
        Future.delayed(const Duration(milliseconds: 700), () => null),
      ]);
      await sub.cancel();
      socket.close();
      return result;
    } catch (_) {
      try { await sub?.cancel(); } catch (_) {}
      try { socket?.close(); } catch (_) {}
      return null;
    }
  }
}

class PolarisFullscreenPage extends StatefulWidget {
  final PolarisClient client;
  final Uint8List initialFrame;
  final int initialWidth;
  final int initialHeight;
  final String fitMode;
  final String performanceMode;
  final bool showStatsOverlay;

  const PolarisFullscreenPage({
    super.key,
    required this.client,
    required this.initialFrame,
    required this.initialWidth,
    required this.initialHeight,
    required this.fitMode,
    required this.performanceMode,
    required this.showStatsOverlay,
  });

  @override
  State<PolarisFullscreenPage> createState() => _PolarisFullscreenPageState();
}

class _PolarisFullscreenPageState extends State<PolarisFullscreenPage> {
  late Uint8List _frame;
  late int _width;
  late int _height;
  bool _cursorVisible = false;
  double _cursorX = 0.5;
  double _cursorY = 0.5;
  late final StreamSubscription<Map<String, dynamic>> _controlSubscription;
  Timer? _controlsTimer;
  bool _showControls = true;
  Timer? _statsHideTimer;
  bool _showStats = true;
  bool? _lastLandscape;

  int _decodeTargetWidth(double logicalWidth, BuildContext context) {
    final px = (logicalWidth * MediaQuery.devicePixelRatioOf(context)).round();
    if (widget.performanceMode == 'Maksimum FPS') return px.clamp(360, 960);
    return px.clamp(360, 1080);
  }

  Future<void> _applyServerOrientation(int width, int height) async {
    if (!mounted || width <= 0 || height <= 0) return;
    final landscape = width > height;
    if (_lastLandscape == landscape) return;
    _lastLandscape = landscape;
    await SystemChrome.setPreferredOrientations(
      landscape
          ? const [DeviceOrientation.landscapeLeft, DeviceOrientation.landscapeRight]
          : const [DeviceOrientation.portraitUp, DeviceOrientation.portraitDown],
    );
  }

  void _armStatsHide() {
    _statsHideTimer?.cancel();
    if (mounted && !_showStats) setState(() => _showStats = true);
    _statsHideTimer = Timer(const Duration(seconds: 5), () {
      if (mounted) setState(() => _showStats = false);
    });
  }

  void _armControlsHide() {
    _controlsTimer?.cancel();
    if (mounted) setState(() => _showControls = true);
    _controlsTimer = Timer(const Duration(seconds: 3), () { if (mounted) setState(() => _showControls = false); });
  }

  @override
  void initState() {
    super.initState();
    _frame = widget.initialFrame;
    _width = widget.initialWidth;
    _height = widget.initialHeight;
    _applyServerOrientation(_width, _height);
    _controlsTimer = Timer(const Duration(seconds: 3), () { if (mounted) setState(() => _showControls = false); });
    _armStatsHide();
    _controlSubscription = widget.client.controls.listen((c) {
      if (!mounted || c['type']?.toString() != 'cursor') return;
      final x = (c['x'] as num?)?.toDouble();
      final y = (c['y'] as num?)?.toDouble();
      setState(() {
        _cursorVisible = c['visible'] == true && x != null && y != null && x >= 0 && y >= 0;
        if (x != null) _cursorX = x.clamp(0.0, 1.0);
        if (y != null) _cursorY = y.clamp(0.0, 1.0);
      });
    });
  }

  @override
  void dispose() {
    _controlsTimer?.cancel();
    _statsHideTimer?.cancel();
    _controlSubscription.cancel();
    // Return the home screen to its normal portrait presentation after leaving viewer.
    SystemChrome.setPreferredOrientations(const [
      DeviceOrientation.portraitUp,
      DeviceOrientation.portraitDown,
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      body: LayoutBuilder(
        builder: (context, constraints) {
          final viewW = constraints.maxWidth;
          final viewH = constraints.maxHeight;
          final sourceAspect = _height > 0 ? _width / _height : 16 / 9;
          double imageW = viewW;
          double imageH = viewW / sourceAspect;
          if (imageH > viewH) {
            imageH = viewH;
            imageW = viewH * sourceAspect;
          }
          final left = (viewW - imageW) / 2;
          final top = (viewH - imageH) / 2;
          return GestureDetector(
          behavior: HitTestBehavior.opaque,
          onTap: () {
            _armControlsHide();
            _armStatsHide();
          },
          child: Stack(
            fit: StackFit.expand,
            children: [
              ValueListenableBuilder<PolarisFrame?>(
                valueListenable: widget.client.latestFrame,
                builder: (context, latest, _) {
                  final live = latest ?? PolarisFrame(_frame, _width, _height, 0);
                  if (latest != null) {
                    final nextWidth = latest.width > 0 ? latest.width : _width;
                    final nextHeight = latest.height > 0 ? latest.height : _height;
                    if (nextWidth != _width || nextHeight != _height) {
                      _width = nextWidth;
                      _height = nextHeight;
                    }
                    // Frame dimensions are authoritative for the live viewer.
                    // This is deliberately driven by frames, not the background
                    // home screen, so Windows rotation changes cannot get stuck.
                    _applyServerOrientation(_width, _height);
                  }
                  final liveAspect = _height > 0 ? _width / _height : sourceAspect;
                  double liveW = viewW;
                  double liveH = viewW / liveAspect;
                  if (liveH > viewH) { liveH = viewH; liveW = viewH * liveAspect; }
                  final liveLeft = (viewW - liveW) / 2;
                  final liveTop = (viewH - liveH) / 2;
                  return Stack(children: [
                    Positioned.fill(child: Center(child: RepaintBoundary(child: _LowLatencyJpeg(frame: live.jpeg, fit: widget.fitMode == 'Doldur' ? BoxFit.cover : BoxFit.contain, targetWidth: _decodeTargetWidth(viewW, context))))),
                    if (_cursorVisible)
                      Positioned(left: (liveLeft + _cursorX * liveW).clamp(0.0, viewW - 1), top: (liveTop + _cursorY * liveH).clamp(0.0, viewH - 1), child: const IgnorePointer(child: _PolarisMouseCursor())),
                  ]);
                },
              ),
              if (_showControls) Positioned(
                top: 14,
                left: 14,
                child: SafeArea(
                  child: Material(
                    color: Colors.black.withOpacity(.55),
                    shape: const CircleBorder(),
                    child: IconButton(
                      tooltip: 'Tam ekrandan çık',
                      onPressed: () => Navigator.of(context).pop(),
                      icon: const Icon(Icons.fullscreen_exit, color: Colors.white, size: 25),
                    ),
                  ),
                ),
              ),
              if (widget.showStatsOverlay && _showStats) Positioned(
                right: 14,
                bottom: 14,
                child: SafeArea(
                  child: ValueListenableBuilder<PolarisStreamStats>(
                    valueListenable: widget.client.stats,
                    builder: (context, st, _) => DecoratedBox(
                      decoration: BoxDecoration(color: Colors.black.withOpacity(.58), borderRadius: BorderRadius.circular(10)),
                      child: Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
                        child: Text('${st.fps} FPS  •  ${st.mbps.toStringAsFixed(1)} Mbps\n$_width × $_height  •  ${st.active ? 'AKIŞ AKTİF' : 'BEKLENİYOR'}', textAlign: TextAlign.right, style: const TextStyle(color: Colors.white70, fontSize: 10, fontWeight: FontWeight.w600)),
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        );
        },
      ),
    );
  }
}

class _PolarisMouseCursor extends StatelessWidget {
  const _PolarisMouseCursor();

  @override
  Widget build(BuildContext context) {
    return CustomPaint(size: const Size(8, 11), painter: _PolarisMouseCursorPainter());
  }
}

class _PolarisMouseCursorPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final path = Path()
      ..moveTo(.45, .45)
      ..lineTo(.45, 9.7)
      ..lineTo(3.05, 7.75)
      ..lineTo(4.8, 10.45)
      ..lineTo(6.0, 9.72)
      ..lineTo(4.35, 7.05)
      ..lineTo(7.75, 7.05)
      ..close();
    final shadow = Paint()..color = Colors.black.withOpacity(.9)..style = PaintingStyle.stroke..strokeWidth = 1.6;
    final fill = Paint()..color = Colors.white;
    canvas.drawPath(path, shadow);
    canvas.drawPath(path, fill);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class _LowLatencyJpeg extends StatefulWidget {
  final Uint8List frame;
  final BoxFit fit;
  final int? targetWidth;
  const _LowLatencyJpeg({required this.frame, required this.fit, this.targetWidth});

  @override
  State<_LowLatencyJpeg> createState() => _LowLatencyJpegState();
}

class _LowLatencyJpegState extends State<_LowLatencyJpeg> {
  ui.Image? _image;
  int _generation = 0;
  Uint8List? _pending;
  bool _decoding = false;

  @override
  void initState() {
    super.initState();
    _pending = widget.frame;
    _decodeLatest();
  }

  @override
  void didUpdateWidget(covariant _LowLatencyJpeg oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (!identical(oldWidget.frame, widget.frame)) {
      _pending = widget.frame;
      _decodeLatest();
    }
  }

  Future<void> _decodeLatest() async {
    if (_decoding || _pending == null) return;
    _decoding = true;
    while (mounted && _pending != null) {
      final bytes = _pending!;
      _pending = null;
      final generation = ++_generation;
      try {
        final codec = await ui.instantiateImageCodec(
          bytes,
          targetWidth: widget.targetWidth != null && widget.targetWidth! > 0 ? widget.targetWidth : null,
        );
        final image = await codec.getNextFrame();
        codec.dispose();
        if (!mounted || generation != _generation) {
          image.image.dispose();
          continue;
        }
        final old = _image;
        setState(() => _image = image.image);
        old?.dispose();
      } catch (_) {
        // A frame can be superseded while decoding. The next frame is preferred.
      }
    }
    _decoding = false;
  }

  @override
  void dispose() {
    _generation++;
    _image?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_image == null) return const ColoredBox(color: Colors.black);
    return RawImage(image: _image, fit: widget.fit, filterQuality: FilterQuality.low, isAntiAlias: false);
  }
}

class _GlassCard extends StatelessWidget {
  final Widget child;
  const _GlassCard({required this.child});
  @override
  Widget build(BuildContext context) => Container(padding: const EdgeInsets.all(16), decoration: BoxDecoration(color: PolarisColors.panel.withOpacity(.92), borderRadius: BorderRadius.circular(18), border: Border.all(color: PolarisColors.blue.withOpacity(.22))), child: child);
}

class _MiniFeature extends StatelessWidget {
  final IconData icon; final String title;
  const _MiniFeature({required this.icon, required this.title});
  @override
  Widget build(BuildContext context) => Container(padding: const EdgeInsets.symmetric(vertical: 13, horizontal: 7), decoration: BoxDecoration(color: PolarisColors.panel, borderRadius: BorderRadius.circular(13), border: Border.all(color: Colors.white.withOpacity(.05))), child: Column(children: [Icon(icon, color: PolarisColors.blue, size: 19), const SizedBox(height: 7), Text(title, textAlign: TextAlign.center, style: const TextStyle(fontSize: 8, fontWeight: FontWeight.w800, letterSpacing: .5, color: PolarisColors.muted))]));
}

class _StatusDot extends StatelessWidget {
  const _StatusDot();
  @override
  Widget build(BuildContext context) => Container(width: 8, height: 8, decoration: const BoxDecoration(color: PolarisColors.success, shape: BoxShape.circle));
}

class _BackgroundGlow extends StatelessWidget {
  const _BackgroundGlow();
  @override
  Widget build(BuildContext context) => IgnorePointer(child: Stack(children: [Positioned(top: -120, right: -100, child: Container(width: 300, height: 300, decoration: BoxDecoration(shape: BoxShape.circle, color: PolarisColors.purple.withOpacity(.10)))), Positioned(bottom: 80, left: -150, child: Container(width: 330, height: 330, decoration: BoxDecoration(shape: BoxShape.circle, color: PolarisColors.blue.withOpacity(.07))))]));
}
