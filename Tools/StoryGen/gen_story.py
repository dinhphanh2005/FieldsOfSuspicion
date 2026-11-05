#!/usr/bin/env python3
import json, os, re, shutil, sys, unicodedata, uuid

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
DATA = os.path.join(ROOT, 'Assets', 'Data')
sys.path.insert(0, os.path.dirname(__file__))
import story as S

SCRIPT = {
    'Day': '95882f6ac5394bcab16c6fcc67c686ce', 'Clue': '7b767cb1fc474758b978a546ca1c709c',
    'Case': 'ec6b87609cfb4ed6ad0997531034a9a7', 'News': '91a76e658d4e486d846779c1b2cde852',
    'Board': 'b7ab85159bff49b3a8ae00f740ba3f8b', 'Ending': 'e4daa1567dbf44a59d24b736a4ae83b3',
}
NS = uuid.UUID('6f0f6b8e-1f6a-4c1e-9a55-0f05f0a5e001')
COND = ['Always', 'DeliveredLastNight', 'NotDeliveredLastNight', 'RouteVisitedLastNight', 'HasFragment', 'DeliveredBy',
        'SuspicionLow', 'SuspicionMid', 'SuspicionHigh', 'CorrectLastNight', 'WrongLastNight', 'CaseCorrect', 'CaseNeverDelivered']
KIND = ['Who', 'What', 'Where', 'When', 'State', 'Evidence', 'Detail']
THREAD = ['Convoy', 'Granary', 'Vaule', 'Route', 'Traitor', 'Report', 'Village']
HANDLER = ['None', 'Rook', 'Shrike']
RISK = ['Safe', 'Low', 'Medium', 'High']
CHANNEL = ['Emel', 'Perrin', 'Sable', 'Patrol']
ENDING = ['Burned', 'Evicted', 'Defection', 'Extraction', 'LeftInCold']


def guid(rel): return uuid.uuid5(NS, 'FoS/' + rel).hex
def q(s): return json.dumps(s if s is not None else '', ensure_ascii=True)
def ref(g): return '{fileID: 11400000, guid: %s, type: 2}' % g


def header(script, name):
    return ('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n'
            '  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n'
            '  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n'
            f'  m_Script: {{fileID: 11500000, guid: {SCRIPT[script]}, type: 3}}\n  m_Name: {name}\n  m_EditorClassIdentifier: \n')


ASSET_META = ('fileFormatVersion: 2\nguid: {g}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n'
              '  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
FOLDER_META = ('fileFormatVersion: 2\nguid: {g}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n'
               '  assetBundleName: \n  assetBundleVariant: \n')


def write_asset(folder, name, body):
    rel = f'Assets/Data/{folder}/{name}.asset'
    g = guid(rel)
    path = os.path.join(ROOT, rel)
    with open(path, 'w', encoding='utf-8', newline='\n') as f: f.write(body)
    with open(path + '.meta', 'w', encoding='utf-8', newline='\n') as f: f.write(ASSET_META.format(g=g))
    return g


def variants(vs, indent):
    if not vs: return '[]'
    pad = ' ' * indent
    out = ''
    for c, p, t in vs:
        out += f'\n{pad}- condition: {COND.index(c)}\n{pad}  param: {q(p)}\n{pad}  text: {q(t)}'
    return out


def line(name, l, indent=4):
    pad = ' ' * indent
    tomas, reply, vs = (l + ([],))[:3] if len(l) == 2 else l
    return (f'{pad}{name}:\n{pad}  tomas: {q(tomas)}\n{pad}  reply: {q(reply)}\n'
            f'{pad}  replyVariants: {variants(vs, indent + 2)}\n')


def strip_accents(s):
    s = s.replace('Đ', 'D').replace('đ', 'd')
    return ''.join(ch for ch in unicodedata.normalize('NFD', s) if unicodedata.category(ch) != 'Mn')


def cipher_of(body):
    sentences = [x.strip() for x in re.split(r'(?<=[.?!])\s+', body.strip()) if x.strip()]
    return ''.join(strip_accents(x[0]).upper() for x in sentences)


def check():
    errors = []
    clue_ids = {c['id'] for c in S.CLUES}
    for m in S.BOARD:
        if m['handler'] != 'None':
            want = m['answer'].replace(' ', '')
            got = cipher_of(m['body'])
            if got != want: errors.append(f"Lệnh {m['name']}: chữ đầu câu = {got}, cần {want}")
    texts = []
    for d in S.DAYS:
        for v in d['visits']:
            for k in ('sell', 'discount', 'ask', 'press', 'farewell'):
                l = v.get(k)
                if l:
                    texts.append((d['day'], v['customer'], l[1]))
                    for vv in (l[2] if len(l) > 2 else []): texts.append((d['day'], v['customer'], vv[2]))
    for day, who, t in texts:
        for cid in re.findall(r'\{\{.+?\|(\w+)\}\}', t or ''):
            if cid not in clue_ids: errors.append(f'N{day} {who}: manh mối {cid} không tồn tại')
    for n in S.NEWS:
        for cid in filter(None, (n.get('grants') or '').split(',')):
            if cid.strip() not in clue_ids: errors.append(f"Tin {n['name']}: grant {cid} không tồn tại")
    kinds = {c['id']: c['kind'] for c in S.CLUES}
    for c in S.CASES:
        for s in c['slots']:
            for cid in s['correct']:
                if cid not in clue_ids: errors.append(f"Hồ sơ {c['id']}: đáp án {cid} không tồn tại")
                elif kinds[cid] != s['kind']: errors.append(f"Hồ sơ {c['id']}: {cid} là {kinds[cid]}, ô cần {s['kind']}")
        if c['template'].count('{') != len(c['slots']): errors.append(f"Hồ sơ {c['id']}: số ô trống không khớp")
    obtainable = set()
    for _, _, t in texts: obtainable |= set(re.findall(r'\{\{.+?\|(\w+)\}\}', t or ''))
    for n in S.NEWS: obtainable |= {x.strip() for x in (n.get('grants') or '').split(',') if x.strip()}
    for c in S.CLUES:
        if c['id'] not in obtainable: errors.append(f"Manh mối {c['id']} không có cách nào lấy được")
    return errors


def main():
    errs = check()
    if errs:
        print('LỖI NỘI DUNG:'); [print(' -', e) for e in errs]; sys.exit(1)

    for folder in ('Clues', 'Cases', 'Board', 'News', 'Days', 'Endings', 'Intel'):
        p = os.path.join(DATA, folder)
        if os.path.isdir(p): shutil.rmtree(p)
        if os.path.exists(p + '.meta'): os.remove(p + '.meta')
    for folder in ('Clues', 'Cases', 'Board', 'News', 'Days', 'Endings'):
        os.makedirs(os.path.join(DATA, folder))
        with open(os.path.join(DATA, folder + '.meta'), 'w', newline='\n') as f:
            f.write(FOLDER_META.format(g=guid('Assets/Data/' + folder)))

    for c in S.CLUES:
        write_asset('Clues', c['id'], header('Clue', c['id']) +
            f"  id: {c['id']}\n  keyword: {q(c['keyword'])}\n  text: {q(c['text'])}\n  kind: {KIND.index(c['kind'])}\n"
            f"  thread: {THREAD.index(c['thread'])}\n  source: {q(c['source'])}\n  isTrue: {1 if c.get('true', True) else 0}\n")

    for c in S.CASES:
        slots = ''.join(f"\n  - label: {q(s['label'])}\n    kind: {KIND.index(s['kind'])}\n    correct:" +
                        ''.join(f"\n    - {x}" for x in s['correct']) for s in c['slots'])
        write_asset('Cases', c['id'], header('Case', c['id']) +
            f"  id: {c['id']}\n  title: {q(c['title'])}\n  brief: {q(c['brief'])}\n  template: {q(c['template'])}\n"
            f"  slots:{slots}\n  opensWith: {q(c.get('opens', ''))}\n  firstNight: {c['first']}\n  lastNight: {c['last']}\n"
            f"  correctSuspicion: {c.get('correctSus', 0)}\n  wrongSuspicion: {c.get('wrongSus', 0)}\n"
            f"  channel: {CHANNEL.index(c.get('channel', 'Emel'))}\n  isFinal: {1 if c.get('final') else 0}\n")

    board_g = {}
    for m in S.BOARD:
        board_g[m['name']] = write_asset('Board', m['name'], header('Board', m['name']) +
            f"  handler: {HANDLER.index(m['handler'])}\n  title: {q(m['title'])}\n  body: {q(m['body'])}\n"
            f"  signature: {q(m['signature'])}\n  answer: {q(m.get('answer', ''))}\n"
            f"  condition: {COND.index(m.get('cond', 'Always'))}\n  param: {q(m.get('param', ''))}\n")

    news_g = {}
    for n in S.NEWS:
        news_g[n['name']] = write_asset('News', n['name'], header('News', n['name']) +
            f"  headline: {q(n['headline'])}\n  sub: {q(n.get('sub', ''))}\n  isSide: {1 if n.get('side') else 0}\n"
            f"  priority: {n.get('priority', 0)}\n  condition: {COND.index(n.get('cond', 'Always'))}\n  param: {q(n.get('param', ''))}\n"
            f"  grantsIntel: {q(n.get('grants', ''))}\n  isLetter: {1 if n.get('letter') else 0}\n  letterNote: {q(n.get('note', ''))}\n")

    cust = {}
    for name in ('Emel', 'Perrin', 'Sable'):
        meta = open(os.path.join(DATA, 'Customers', name + '.asset.meta')).read()
        cust[name] = re.search(r'guid: (\w+)', meta).group(1)

    for d in S.DAYS:
        body = header('Day', f"N{d['day']}") + f"  day: {d['day']}\n"
        body += '  news:' + (''.join(f"\n  - {ref(news_g[x])}" for x in d['news']) or ' []') + '\n'
        body += '  board:' + (''.join(f"\n  - {ref(board_g[x])}" for x in d['board']) or ' []') + '\n'
        body += '  visits:'
        if not d['visits']: body += ' []'
        for v in d['visits']:
            body += (f"\n  - customer: {ref(cust[v['customer']])}\n    requiredMomentum: 0\n"
                     f"    entrance: {q(v.get('entrance', ''))}\n    entranceVariants: {variants(v.get('entranceV'), 4)}\n"
                     f"    greeting: {q(v['greeting'])}\n    greetingVariants: {variants(v.get('greetingV'), 4)}\n"
                     f"    askCost: {v.get('askCost', 0)}\n    sellPrice: {v.get('sellPrice', 7)}\n    discountPrice: {v.get('discountPrice', 4)}\n"
                     f"    sellQuantity: {v.get('sellQty', 1)}\n    discountQuantity: {v.get('discountQty', 1)}\n"
                     f"    discountActionLabel: {q(v.get('discountLabel', ''))}\n    discountAccepted: {0 if v.get('noDiscount') else 1}\n"
                     f"    pressSuspicion: {v.get('pressSus', 1)}\n")
            for k in ('sell', 'discount', 'ask', 'press', 'farewell'):
                body += line(k, v[k])
            body += f"    exit: {q(v.get('exit', ''))}\n    exitVariants: {variants(v.get('exitV'), 4)}"
        body += '\n  night:'
        if not d.get('night'): body += ' []'
        for r in d.get('night', []):
            body += (f"\n  - a: {r['a']}\n    b: {r['b']}\n    risk: {RISK.index(r.get('risk', 'Safe'))}\n"
                     f"    closed: {1 if r.get('closed') else 0}\n    hiddenUntil: {q(r.get('hidden', ''))}\n    note: {q(r.get('note', ''))}")
        write_asset('Days', f"N{d['day']}", body + '\n')

    for e in S.ENDINGS:
        write_asset('Endings', 'Ending_' + e['id'], header('Ending', 'Ending_' + e['id']) +
            f"  id: {ENDING.index(e['id'])}\n  title: {q(e['title'])}\n  text: {q(e['text'])}\n"
            f"  strongEvidence: {q(e.get('strong', ''))}\n  weakEvidence: {q(e.get('weak', ''))}\n")

    print(f'OK: {len(S.CLUES)} manh mối, {len(S.CASES)} hồ sơ, {len(S.BOARD)} thẻ bảng tin, '
          f'{len(S.NEWS)} tin, {len(S.DAYS)} ngày, {len(S.ENDINGS)} kết thúc. Mọi lệnh mật đã kiểm tra.')


if __name__ == '__main__':
    main()
