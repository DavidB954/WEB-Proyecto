import zlib, base64, urllib.request, sys
 
def plantuml_encode(text):
    compressed = zlib.compress(text.encode('utf-8'))
    return base64.urlsafe_b64encode(compressed).decode('ascii')
 
puml_file = sys.argv[1]
out_png   = sys.argv[2]
out_svg   = sys.argv[3] if len(sys.argv) > 3 else None
 
with open(puml_file, encoding='utf-8') as f:
    content = f.read()
 
encoded = plantuml_encode(content)
 
# Descargar PNG
url_png = f'https://kroki.io/plantuml/png/{encoded}'
req = urllib.request.Request(url_png, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req, timeout=60) as resp:
    data = resp.read()
with open(out_png, 'wb') as f:
    f.write(data)
print(f'PNG: {out_png} ({len(data)} bytes)')
 
# Descargar SVG para verificar que no se corta
if out_svg:
    url_svg = f'https://kroki.io/plantuml/svg/{encoded}'
    req2 = urllib.request.Request(url_svg, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req2, timeout=60) as resp2:
        svg_data = resp2.read()
    with open(out_svg, 'wb') as f:
        f.write(svg_data)
    print(f'SVG: {out_svg} ({len(svg_data)} bytes)')