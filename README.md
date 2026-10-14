# RollerAgent

ML-Agents Unity paketi `Packages/manifest.json` içinde yerel bir klasörden yüklenir. Projeyi açmadan önce ML-Agents deposunu klonlayıp 4.1.0 sürümüne geçin ve `com.unity.ml-agents` satırındaki yolu kendi klonunuza göre düzeltin:

```bash
git clone https://github.com/Unity-Technologies/ml-agents.git
cd ml-agents && git checkout ee0a08ccae597094003844d0121317f9790a1676
```

```json
"com.unity.ml-agents": "file:/path/to/ml-agents/com.unity.ml-agents"
```
