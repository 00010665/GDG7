"""Create the project's small, clean food-themed UI sprite set (stdlib only)."""
import math, os, struct, zlib

OUT = os.path.dirname(__file__)
N = 128
icons = {
 'Chef_Diego':'chef_mex', 'Chef_Kenji':'chef_jp', 'Chef_Mario':'chef_it',
 'BigPortion':'plate', 'CheeseArmor':'cheese', 'ComfortShoes':'shoes', 'FastService':'cloche',
 'FreshIngredients':'basket', 'MagnetApron':'apron', 'SpicyPepper':'pepper', 'TipJar':'jar',
 'WasabiBoost':'wasabi', 'PastaLauncher':'donutgun', 'PizzaCutter':'cakecutter',
 'SalsaSplash':'pinepizza', 'SashimiSlice':'sushiroll', 'SushiSet':'ribs', 'TacoThrow':'taco',
 'TeaPot':'hotdog', 'MegaBurger':'burger'
}
def color(h): return tuple(bytes.fromhex(h.lstrip('#')))+(255,)
def render(kind):
 p=[(0,0,0,0)]*(N*N)
 def put(x,y,c):
  if 0<=x<N and 0<=y<N: p[y*N+x]=color(c) if isinstance(c,str) else c
 def circle(cx,cy,r,c):
  rr=r*r
  for y in range(max(0,cy-r),min(N,cy+r+1)):
   for x in range(max(0,cx-r),min(N,cx+r+1)):
    if (x-cx)**2+(y-cy)**2<=rr: put(x,y,c)
 def line(x1,y1,x2,y2,c,w=4):
  steps=max(abs(x2-x1),abs(y2-y1),1)
  for i in range(steps+1): circle(round(x1+(x2-x1)*i/steps),round(y1+(y2-y1)*i/steps),w//2,c)
 def poly(points,c):
  for y in range(max(0,min(y for _,y in points)),min(N,max(y for _,y in points)+1)):
   for x in range(max(0,min(x for x,_ in points)),min(N,max(x for x,_ in points)+1)):
    inside=False
    for i,(ax,ay) in enumerate(points):
     bx,by=points[(i+1)%len(points)]
     if (ay>y)!=(by>y) and x<(bx-ax)*(y-ay)/(by-ay+1e-9)+ax: inside=not inside
    if inside: put(x,y,c)
 # warm framed badge
 circle(64,64,61,'#E5B653'); circle(64,64,56,'#26344A')
 if kind.startswith('chef'):
  circle(64,59,34 if kind=='chef_it' else 31,'#F1B887')
  if kind=='chef_mex':
   poly([(24,43),(35,32),(50,28),(54,16),(72,16),(78,29),(94,33),(105,43),(96,49),(33,49)],'#F8F6EF')
   line(31,43,99,43,'#4E9A5C',4); line(37,48,92,48,'#C94E43',3)
  elif kind=='chef_jp':
   poly([(31,40),(34,27),(46,30),(50,18),(65,25),(76,17),(83,31),(98,27),(99,46)],'#F8F6EF')
   line(35,39,94,39,'#B83C43',5)
  else:
   poly([(34,45),(35,32),(44,28),(43,14),(53,10),(57,25),(65,9),(72,9),(76,26),(84,13),(93,17),(89,32),(96,43)],'#FFFFFF')
  poly([(30,83),(40,72),(64,79),(87,72),(99,84),(106,119),(22,119)],'#377B69' if kind=='chef_mex' else ('#B94A42' if kind=='chef_it' else '#557D91'))
  circle(53,59,3,'#293248'); circle(75,59,3,'#293248'); line(56,72,73,72,'#A84E43',3)
  if kind=='chef_mex':
   line(48,65,60,70,'#50372D',4); line(60,70,64,66,'#50372D',4); line(64,66,69,70,'#50372D',4); line(69,70,81,65,'#50372D',4)
   poly([(51,78),(64,85),(77,78),(72,91),(64,96),(56,91)],'#E6AF42')
  elif kind=='chef_it':
   circle(45,67,7,'#E99D83'); circle(83,67,7,'#E99D83'); line(44,67,56,71,'#352B2B',6); line(56,71,64,67,'#352B2B',5); line(64,67,72,71,'#352B2B',5); line(72,71,84,67,'#352B2B',6)
  else: line(53,67,75,67,'#50372D',4)
 elif kind=='plate':
  circle(64,76,39,'#EEECE4'); circle(64,76,31,'#72A95C'); circle(48,68,10,'#E5B951'); circle(73,61,11,'#CE624A'); circle(80,84,9,'#E7D180'); circle(53,88,8,'#F1DFC0')
 elif kind=='cheese':
  poly([(31,44),(75,32),(100,48),(94,91),(65,109),(35,91)],'#F1C550')
  for x,y,r in [(50,57,5),(75,51,6),(85,73,5),(56,83,7),(76,93,4)]: circle(x,y,r,'#C99636')
 elif kind=='shoes':
  poly([(28,72),(41,63),(59,65),(69,82),(91,87),(105,98),(101,108),(30,108)],'#5CC9D3'); line(42,84,65,91,'#F4F3E9',4); line(38,93,60,99,'#F4F3E9',4); line(34,112,101,112,'#E4B652',4)
 elif kind=='cloche':
  circle(64,68,32,'#CAD6DA'); circle(64,68,24,'#33435A'); line(64,66,64,47,'#F2C84B',5); line(64,66,78,73,'#F2C84B',5); line(34,91,94,91,'#DDE4E5',6); circle(64,37,5,'#F2C84B')
 elif kind=='basket':
  poly([(32,63),(96,63),(88,104),(40,104)],'#A86F45'); line(35,73,93,73,'#E0AF71',4); line(42,86,88,86,'#E0AF71',4); line(48,63,48,102,'#E0AF71',4)
  for x,y,c in [(44,56,'#E5684E'),(61,42,'#6BB45D'),(78,53,'#E8C64D'),(66,56,'#73B954')]: circle(x,y,12,c)
 elif kind=='apron':
  poly([(44,36),(55,46),(73,46),(84,36),(96,51),(85,63),(91,104),(37,104),(43,63),(32,51)],'#EFF1ED'); line(46,56,46,77,'#DF4A4A',6); line(82,56,82,77,'#DF4A4A',6); line(46,77,51,85,'#DF4A4A',6); line(82,77,77,85,'#DF4A4A',6)
 elif kind=='pepper':
  poly([(45,43),(62,38),(80,51),(86,69),(76,92),(62,106),(50,91),(39,68)],'#E84739'); line(63,41,66,27,'#69A653',6); line(66,27,82,31,'#69A653',5); line(31,76,21,70,'#F2B848',4); line(94,82,105,77,'#F2B848',4)
 elif kind=='jar':
  poly([(43,44),(85,44),(91,99),(38,99)],'#77C9D4'); line(39,44,89,44,'#ECF1EE',7)
  for x,y in [(51,73),(66,62),(77,82),(58,89)]: circle(x,y,7,'#F1C64E')
 elif kind=='wasabi':
  for x,y,r in [(48,77,24),(66,61,25),(83,77,20),(65,84,24)]: circle(x,y,r,'#80BC55')
  line(28,53,20,45,'#DFF279',3); line(97,52,106,44,'#DFF279',3); line(100,92,110,97,'#DFF279',3)
 elif kind=='donutgun':
  poly([(29,66),(72,56),(90,66),(90,82),(51,91),(36,82)],'#D99E54'); circle(87,72,17,'#EBC778'); circle(87,72,7,'#29374D'); poly([(38,82),(56,78),(62,101),(43,105)],'#986343')
  for x,y,c in [(44,44,'#F4B957'),(67,36,'#EE7A75'),(90,44,'#E8A4C2')]: circle(x,y,12,c); circle(x,y,4,'#26344A')
 elif kind=='cakecutter':
  circle(54,64,30,'#B65B47'); circle(54,64,21,'#F1D68B'); circle(54,64,13,'#75493E'); line(73,81,99,106,'#C5D0D5',10); circle(54,64,7,'#E9B653')
 elif kind=='pinepizza':
  circle(64,68,39,'#E1A446'); circle(64,68,31,'#F4D982'); circle(50,56,7,'#D55043'); circle(75,57,7,'#D55043'); circle(59,81,7,'#D55043'); circle(81,78,6,'#73A956')
 elif kind=='sushiroll':
  for x,y in [(43,55),(82,55),(61,88),(89,91),(47,94)]: circle(x,y,17,'#F2EEE2'); circle(x,y,8,'#E46958'); circle(x,y,4,'#69A85A')
 elif kind=='ribs':
  poly([(32,45),(89,48),(99,62),(89,76),(44,72)],'#9D573A')
  for x in [47,60,73,86]: line(x,54,x+2,88,'#F2D7A4',5)
  line(37,97,88,97,'#C96942',5)
 elif kind=='taco':
  poly([(28,83),(36,54),(58,38),(82,43),(101,62),(100,83),(79,96),(49,95)],'#E8BB53'); poly([(37,75),(47,54),(65,47),(85,55),(94,75),(80,85),(54,84)],'#70A754'); line(47,70,84,68,'#B85542',7)
 elif kind=='hotdog':
  poly([(25,57),(100,49),(104,83),(31,91)],'#D5984F'); line(37,67,92,63,'#B95F43',13); line(41,71,91,66,'#EFC44A',4)
 elif kind=='burger':
  poly([(33,64),(42,48),(63,41),(86,48),(96,64)],'#D99F4D'); line(34,66,96,66,'#E4C14A',7); line(36,76,94,76,'#65A555',8); line(38,84,91,84,'#A84D3D',9); poly([(38,90),(91,90),(84,105),(46,105)],'#D99F4D')
 raw=b''.join(b'\0'+bytes(sum((list(px) for px in p[y*N:(y+1)*N]),[])) for y in range(N))
 def chunk(t,d): return struct.pack('!I',len(d))+t+d+struct.pack('!I',zlib.crc32(t+d)&0xffffffff)
 return b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('!2I5B',N,N,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(raw,9))+chunk(b'IEND',b'')
for filename,kind in icons.items():
 path=os.path.join(OUT,filename+'.png')
 open(path,'wb').write(render(kind))
 # Unity imports these directly as UI sprites.
 import uuid
 guid=uuid.uuid4().hex
 meta=f'''fileFormatVersion: 2\nguid: {guid}\nTextureImporter:\n  internalIDToNameTable: []\n  externalObjects: {{}}\n  serializedVersion: 13\n  mipmaps:\n    mipMapMode: 0\n    enableMipMap: 0\n    sRGBTexture: 1\n    linearTexture: 0\n    fadeOut: 0\n    borderMipMap: 0\n    mipMapsPreserveCoverage: 0\n    alphaTestReferenceValue: 0.5\n    mipMapFadeDistanceStart: 1\n    mipMapFadeDistanceEnd: 3\n  bumpmap:\n    convertToNormalMap: 0\n    externalNormalMap: 0\n    heightScale: 0.25\n    normalMapFilter: 0\n    flipGreenChannel: 0\n  isReadable: 0\n  streamingMipmaps: 0\n  streamingMipmapsPriority: 0\n  vTOnly: 0\n  ignoreMipmapLimit: 0\n  grayScaleToAlpha: 0\n  generateCubemap: 6\n  cubemapConvolution: 0\n  seamlessCubemap: 0\n  textureFormat: 1\n  maxTextureSize: 256\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  lightmap: 0\n  spriteMode: 1\n  spriteExtrude: 1\n  spriteMeshType: 1\n  alignment: 0\n  spritePivot: {{x: 0.5, y: 0.5}}\n  spritePixelsToUnits: 100\n  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}\n  spriteGenerateFallbackPhysicsShape: 1\n  alphaUsage: 1\n  alphaIsTransparency: 1\n  textureType: 8\n  textureShape: 1\n  singleChannelComponent: 0\n  maxTextureSizeSet: 0\n  compressionQualitySet: 0\n  textureFormatSet: 0\n  ignorePngGamma: 0\n  applyGammaDecoding: 0\n  spriteSheet:\n    serializedVersion: 2\n    sprites: []\n    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: {uuid.uuid4().hex}\n    internalID: 0\n    vertices: []\n    indices: \n    edges: []\n    weights: []\n  secondarySpriteTextures: []\n  nameFileIdTable: {{}}\n  spritePackingTag: \n  pSDRemoveMatte: 0\n  pSDShowRemoveMatteOption: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'''
 open(path+'.meta','w',newline='\n').write(meta)
print(f'Generated {len(icons)} icons in {OUT}')
