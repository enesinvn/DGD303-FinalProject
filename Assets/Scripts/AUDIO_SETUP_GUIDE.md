# 🎵 SES SİSTEMİ KURULUM REHBERİ

Bu rehber, oyununuzdaki ses efektlerini ve müzikleri Unity Editor'da nasıl yerleştireceğinizi adım adım açıklar.

---

## 📋 İÇİNDEKİLER
1. [AudioManager Kurulumu](#1-audiomanager-kurulumu)
2. [EnemyAI Ses Atamaları](#2-enemyai-ses-atamaları)
3. [Kapı Sesleri](#3-kapı-sesleri)
4. [Player Sesleri](#4-player-sesleri)
5. [UI Button Sesleri](#5-ui-button-sesleri)

---

## 1. AudioManager Kurulumu

### Adım 1: GameObject Oluştur
1. **Hierarchy** panelinde sağ tık → **Create Empty**
2. İsmi `AudioManager` olarak değiştir
3. `AudioManager.cs` script'ini bu GameObject'e ekle (Add Component)

### Adım 2: Müzikleri Ata

#### Normal Background Music
- **Dosya**: `Assets/Sounds/774883__destructo20__background-music-of-uncertainty.wav`
- **Inspector'da**: `Normal Background Music` alanına sürükle
- **Ayarlar**: 
  - Music Volume: `0.3`
  - Loop: `✓` (otomatik ayarlanır)

#### Tension Music (Arama/Araştırma)
- **Dosya**: `Assets/Sounds/BackgroundMusic.ogg` veya `774883__destructo20__background-music-of-uncertainty.wav`
- **Inspector'da**: `Tension Music` alanına sürükle

#### Chase Music (Kovalama)
- **Dosya**: `Assets/Sounds/421606__jesskawaiixxx__tension-and-chase-disturbed-horror-loop.mp3`
- **Inspector'da**: `Chase Music` alanına sürükle
- **Not**: Bu müzik düşman kovaladığında otomatik çalar

#### Horror Ambient (Atmosfer)
- **Dosya**: `Assets/Sounds/193692__julius_galla__atmosphere-horror-1-loop.wav`
- **Inspector'da**: `Horror Ambient` alanına sürükle
- **Ayarlar**: 
  - Ambient Volume: `0.2`
  - Loop: `✓` (otomatik ayarlanır)

### Adım 3: UI ve Efekt Sesleri

#### Button Click Sound
- **Dosya**: `Assets/Sounds/Button_click.wav`
- **Inspector'da**: `Button Click Sound` alanına sürükle

#### Button Hover Sound (Opsiyonel)
- **Dosya**: `Assets/Sounds/Button_click.wav` (veya daha hafif bir ses)
- **Inspector'da**: `Button Hover Sound` alanına sürükle
- **Not**: Mouse buton üzerine geldiğinde çalar (daha düşük volume ile)

#### Victory Sound
- **Dosya**: Kendi seçiminiz (örn: `658431__deathbyfairydust__pop.wav`)
- **Inspector'da**: `Victory Sound` alanına sürükle

#### Defeat Sound
- **Dosya**: `Assets/Sounds/126113__klankbeeld__laugh.wav` (korkunç bir kahkaha)
- **Inspector'da**: `Defeat Sound` alanına sürükle

#### Breath Sound
- **Dosya**: `Assets/Sounds/817418__flavioconcini__sound-breath.mp3`
- **Inspector'da**: `Breath Sound` alanına sürükle

#### Item Pickup Sound
- **Dosya**: `Assets/Sounds/387133__rdaly95__collecting_health.wav`
- **Inspector'da**: `Item Pickup Sound` alanına sürükle

#### Door Lock Sound
- **Dosya**: `Assets/Sounds/158626__mrauralization__door-lock.wav`
- **Inspector'da**: `Door Lock Sound` alanına sürükle

---

## 2. EnemyAI Ses Atamaları

Her düşman GameObject'ine bu sesleri ekleyin:

### Adım 1: Enemy GameObject'ini Seç
- **Hierarchy**'de düşman GameObject'inizi seçin (örn: "Enemy", "Monster", vs.)

### Adım 2: Sesleri Ata

#### Idle Sounds (Devriye Sesleri)
Düşman normal devriye yaparken çalacak sesler (5 saniyede bir):
- `Assets/Sounds/502504__rudmer_rotteveel__wood-creak-single-v2.wav` (Tahta gıcırtısı)
- `Assets/Sounds/529952__beetlemuse__door-creak-penguin-snow-globe-game.wav` (Kapı gıcırtısı)

**Inspector'da**: 
1. `Idle Sounds` array'ini genişlet
2. Size = `2` yap
3. İki ses dosyasını sürükle

#### Alert Sounds (Oyuncuyu Fark Edince)
Düşman oyuncuyu ilk gördüğünde çalan ses:
- `Assets/Sounds/270465__littlerobotsoundfactory__laugh_evil_00.wav` (Kötü kahkaha)

**Inspector'da**: 
1. `Alert Sounds` array'ini genişlet
2. Size = `1` yap
3. Ses dosyasını sürükle

#### Chase Sounds (Kovalama Sesleri)
Düşman oyuncuyu kovalarken çalan sesler:
- `Assets/Sounds/577103__ninushideon__evil-maniac-laught.wav` (Manyak kahkaha)
- `Assets/Sounds/126113__klankbeeld__laugh.wav` (Kahkaha)

**Inspector'da**: 
1. `Chase Sounds` array'ini genişlet
2. Size = `2` yap
3. İki ses dosyasını sürükle

#### Attack Sound (Saldırı)
Düşman saldırırken çalan ses:
- `Assets/Sounds/577103__ninushideon__evil-maniac-laught.wav` (Korkunç kahkaha)

**Inspector'da**: 
1. `Attack Sound` alanına sürükle

#### Kill Sound (Öldürme)
Oyuncuyu yakaladığında çalan ses:
- `Assets/Sounds/126113__klankbeeld__laugh.wav` (Kazanma kahkahası)

**Inspector'da**: 
1. `Kill Sound` alanına sürükle

### Adım 3: Ses Ayarları
- `Sound Interval`: `5` (saniye) - Idle/Chase sesleri arası bekleme
- `AudioSource` → **3D Sound Settings**:
  - Spatial Blend: `1.0` (Tam 3D)
  - Max Distance: `25`
  - Min Distance: `5`

---

## 3. Kapı Sesleri

Her kapı GameObject'ine bu sesleri ekleyin:

### Adım 1: Door GameObject'ini Seç
- **Hierarchy**'de kapı GameObject'inizi seçin

### Adım 2: Sesleri Ata

#### Open Sound
- **Dosya**: `Assets/Sounds/15419__pagancow__dorm-door-opening.wav`
- **Inspector'da**: `Open Sound` alanına sürükle

#### Close Sound
- **Dosya**: `Assets/Sounds/doorOpen_1.ogg` veya `doorOpen_2.ogg`
- **Inspector'da**: `Close Sound` alanına sürükle

#### Locked Sound
- **Dosya**: `Assets/Sounds/158626__mrauralization__door-lock.wav`
- **Inspector'da**: `Locked Sound` alanına sürükle

### Adım 3: AudioSource Ayarları
Kapıda zaten AudioSource varsa ayarlayın:
- Spatial Blend: `1.0` (Tam 3D)
- Max Distance: `10`
- Play On Awake: `✗` (Kapalı)

---

## 4. Player Sesleri

### PlayerHiding Script'i

#### Adım 1: Player GameObject'ini Seç
- **Hierarchy**'de Player GameObject'inizi seçin

#### Adım 2: Ses Ayarları

##### Normal Breathing Sound
- **Dosya**: `Assets/Sounds/817418__flavioconcini__sound-breath.mp3`
- **Inspector'da**: `Normal Breathing Sound` alanına sürükle

##### Holding Breath Sound
- **Dosya**: `Assets/Sounds/817418__flavioconcini__sound-breath.mp3` (daha düşük volume ile)
- **Inspector'da**: `Holding Breath Sound` alanına sürükle

##### Gasping Sound
- **Dosya**: `Assets/Sounds/817418__flavioconcini__sound-breath.mp3`
- **Inspector'da**: `Gasping Sound` alanına sürükle

##### Breathing Audio Source
1. Player'a yeni bir **Empty Child Object** ekle, ismi: `BreathingSource`
2. AudioSource component ekle:
   - Spatial Blend: `0` (2D - sadece oyuncu duyar)
   - Volume: `0.5`
3. Bu AudioSource'u `Breathing Audio Source` alanına sürükle

---

## 5. UI Button Sesleri

### Adım 1: Menu Manager GameObject'ini Bul
- **Hierarchy**'de `MainMenuManager` veya `PauseManager` GameObject'lerini bulun

### Adım 2: Button'lara Ses Ekle

#### Her Button için:
1. Button GameObject'ini seç
2. Inspector → **Button** component
3. **On Click ()** event'inde yeni event ekle:
   - Runtime → `AudioManager`
   - Function → `AudioManager.PlayButtonClick()`

#### Button Click Sound
- **Dosya**: `Assets/Sounds/Button_click.wav`
- AudioManager'da zaten atandı (yukarıda)

---

## 🎮 TEST ETME

### Müzik Geçişleri Test Et
1. **Play** butonuna bas
2. Normal müzik çalmalı (arka planda hafif korku müziği)
3. Düşmana yaklaş → Tension Music'e geçmeli
4. Düşman seni fark edince → Chase Music'e geçmeli
5. Düşman seni kaybedince → Tension → Normal'e dönmeli

### Düşman Sesleri Test Et
1. Düşman devriye yaparken → Idle Sounds (tahta gıcırtısı)
2. Seni görünce → Alert Sound (kötü kahkaha)
3. Kovalarken → Chase Sounds (manyak kahkaha)
4. Yakaladığında → Kill Sound (kazanma kahkahası)

### Kapı Sesleri Test Et
1. Kapıyı aç → Open Sound (kapı açılma)
2. Kapıyı kapat → Close Sound (kapı kapanma)
3. Kilitli kapıya tıkla → Locked Sound (kilit sesi)

### Player Sesleri Test Et
1. Hiding spot'a gir
2. Space tuşuna bas → Breath holding sound
3. Space'i bırak → Normal breathing
4. Nefesi bitir → Gasping sound

---

## 🔧 SORUN GİDERME

### Müzik Çalmıyor
- AudioManager GameObject'inin scene'de olduğundan emin ol
- AudioManager'ın child objelerinde 3 AudioSource olmalı:
  - MusicSource
  - AmbientSource
  - UISource

### Sesler Çok Yüksek/Alçak
- AudioManager → `Music Volume`: 0.1 - 0.5 arası dene
- AudioManager → `Ambient Volume`: 0.1 - 0.3 arası dene
- Master Volume → Edit → Project Settings → Audio → Volume: 0.8

### Düşman Sesleri Çalışmıyor
- Enemy GameObject'inde `AudioSource` component olduğundan emin ol
- Audio Source → 3D Settings:
  - Spatial Blend = 1.0
  - Max Distance = 25
  - Rolloff = Linear

### Kapı Sesi Gecikiyor
- Door script'inde `soundTriggerProgress = 0.15f` değerini 0.05'e düşür
- Bu, sesin kapı hareketi başlar başlamaz çalmasını sağlar

---

## ✅ TAMAMLANMIŞ KONTROL LİSTESİ

- [ ] AudioManager GameObject oluşturuldu
- [ ] AudioManager'a tüm müzikler atandı
- [ ] AudioManager'a UI sesleri atandı
- [ ] AudioManager'a efekt sesleri atandı
- [ ] Enemy AI'a tüm sesler atandı (idle, alert, chase, attack, kill)
- [ ] Door'lara tüm sesler atandı (open, close, locked)
- [ ] Player'a breathing sesleri atandı
- [ ] Button'lara click sesleri bağlandı
- [ ] Test edildi ve çalışıyor ✓

---

## 📝 NOTLAR

- **Loop Müzikler**: Background ve Ambient müzikler loop olmalı (AudioClip ayarlarında loop işaretle)
- **3D vs 2D Sesler**: 
  - Enemy, Door sesleri → 3D (Spatial Blend = 1.0)
  - UI, Player breathing → 2D (Spatial Blend = 0)
- **Volume Dengesi**:
  - Background Music: 0.3
  - Ambient: 0.2
  - UI: 0.7
  - SFX: 0.5-1.0

**İyi Oyunlar!** 🎮👻
