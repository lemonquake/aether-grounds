from pathlib import Path
p=Path(__file__).with_name('integrate_overhaul.py')
s=p.read_text(encoding='utf-8')
exec(compile(s[:s.index("edit('Assets/Scripts/ModelLibrary.cs'")]+s[s.index("p=root/'Assets/Scripts/Game.cs';s=p.read_text();s=s.replace"):],str(p),'exec'))
