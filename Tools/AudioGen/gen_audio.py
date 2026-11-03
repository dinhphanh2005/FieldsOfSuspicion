#!/usr/bin/env python3
"""Tạo toàn bộ âm thanh của game bằng code (không dùng file của người khác → không vướng bản quyền).

Chạy từ thư mục gốc project:  python3 Tools/AudioGen/gen_audio.py
Ra: Assets/Resources/Audio/{music_day, music_night, sfx_click, sfx_page, sfx_bell, sfx_pen, sfx_drop}.wav
Chỉ cần numpy. Nhạc là vòng lặp liền mạch (đuôi vang được cộng vòng về đầu).
"""
import os, wave
import numpy as np

SR = 32000
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Audio')
rng = np.random.default_rng(7)


def midi(n): return 440.0 * 2 ** ((n - 69) / 12)


def fft_convolve(a, b):
    n = len(a) + len(b) - 1
    N = 1 << (n - 1).bit_length()
    return np.fft.irfft(np.fft.rfft(a, N) * np.fft.rfft(b, N), N)[:n]


def lowpass(x, cutoff, taps=255):
    t = np.arange(taps) - (taps - 1) / 2
    h = np.sinc(2 * cutoff / SR * t) * np.hamming(taps)
    return fft_convolve(x, h / h.sum())[(taps - 1) // 2:][:len(x)]


def highpass(x, cutoff, taps=255):
    return x - lowpass(x, cutoff, taps)


def reverb(x, seconds=1.8, mix=0.3):
    n = int(seconds * SR)
    t = np.arange(n) / SR
    ir = rng.standard_normal(n) * np.exp(-t * 6.0 / seconds)
    ir = lowpass(ir, 5000)
    ir /= np.sqrt((ir ** 2).sum())
    wet = fft_convolve(x, ir)
    out = np.zeros(len(wet)); out[:len(x)] = x * (1 - mix)
    return out + wet * mix


def env_adsr(n, a, d, s, r):
    e = np.ones(n) * s
    A, D, R = int(a * SR), int(d * SR), int(r * SR)
    e[:A] = np.linspace(0, 1, A, endpoint=False)
    e[A:A + D] = np.linspace(1, s, D, endpoint=False)
    if R > 0: e[-R:] *= np.linspace(1, 0, R)
    return e


def pluck(freq, dur, bright=0.5):
    n = int(dur * SR); t = np.arange(n) / SR
    s = sum((bright ** (k - 1)) / k * np.sin(2 * np.pi * freq * k * t) for k in range(1, 7))
    return s * np.exp(-t * 3.2) * env_adsr(n, 0.004, 0.0, 1.0, 0.05)


def pad(freq, dur):
    n = int(dur * SR); t = np.arange(n) / SR
    s = np.zeros(n)
    for det in (-0.12, 0.0, 0.11):   # ba dao động lệch nhẹ → dày, ấm
        f = freq * 2 ** (det / 12)
        s += np.sin(2 * np.pi * f * t) + 0.3 * np.sin(2 * np.pi * 2 * f * t) + 0.12 * np.sin(2 * np.pi * 3 * f * t)
    return s / 3 * env_adsr(n, dur * 0.35, 0.0, 1.0, dur * 0.4)


def bell(freq, dur):
    n = int(dur * SR); t = np.arange(n) / SR
    parts = [(1, 1.0, 1.2), (2.76, 0.5, 2.2), (5.40, 0.25, 3.5), (8.93, 0.12, 5.0)]
    return sum(a * np.sin(2 * np.pi * freq * r * t) * np.exp(-t * k) for r, a, k in parts) * env_adsr(n, 0.002, 0, 1, 0.02)


def place(buf, sig, at):
    i = int(at * SR)
    end = min(len(buf), i + len(sig))
    buf[i:end] += sig[:end - i]


def loop_track(total, render):
    """Dựng track dài `total` giây + 4 giây đuôi, rồi gập đuôi về đầu → lặp không có khe."""
    buf = np.zeros(int((total + 4) * SR))
    render(buf)
    buf = reverb(buf, 2.2, 0.35)[:len(buf)]
    n = int(total * SR)
    out = buf[:n].copy()
    out[:len(buf) - n] += buf[n:]
    return out


def music_day():
    bpm = 80; beat = 60 / bpm; bar = 4 * beat
    # Rê trưởng, vòng I – vi – IV – V (D – Bm – G – A), mộc mạc kiểu làng quê
    chords = [[50, 54, 57], [47, 50, 54], [43, 47, 50], [45, 49, 52]]
    arp = [0, 1, 2, 1, 0, 2, 1, 2]
    melody = [[74, None, 76, 78, None, 76, 74, None], [71, None, 74, None, 73, 71, None, None],
              [71, None, 74, 76, None, 74, 71, None], [73, None, 76, None, 73, None, 69, None]]
    bars = 16

    def render(buf):
        for b in range(bars):
            ch = chords[b % 4]; t0 = b * bar
            place(buf, 0.10 * pad(midi(ch[0] - 12), bar + 1.0), t0)
            for i, k in enumerate(arp):
                place(buf, 0.16 * pluck(midi(ch[k] + 12), 1.6, 0.45), t0 + i * beat / 2)
            if b >= 4 and (b // 4) % 2 == 1:   # giai điệu vào ở vòng 2 và 4
                for i, m in enumerate(melody[b % 4]):
                    if m: place(buf, 0.12 * pluck(midi(m), 1.2, 0.3), t0 + i * beat / 2)
    return loop_track(bars * bar, render)


def music_night():
    bpm = 56; beat = 60 / bpm; bar = 4 * beat
    # La thứ, vòng i – VI – III – VII (Am – F – C – G), chậm và hồi hộp
    chords = [[45, 48, 52], [41, 45, 48], [48, 52, 55], [43, 47, 50]]
    bars = 12

    def render(buf):
        for b in range(bars):
            ch = chords[b % 4]; t0 = b * bar
            for note in ch:
                place(buf, 0.07 * pad(midi(note), bar + 1.5), t0)
            place(buf, 0.10 * pad(midi(ch[0] - 12), bar + 1.5), t0)
            if b % 2 == 1:   # tiếng chuông xa, thưa
                place(buf, 0.05 * bell(midi(ch[2] + 24), 3.0), t0 + beat * 2.5)
        # tiếng côn trùng đêm rất nhỏ
        n = len(buf); t = np.arange(n) / SR
        crick = highpass(rng.standard_normal(n), 4500) * (0.5 + 0.5 * np.sign(np.sin(2 * np.pi * 7 * t))) \
                * (0.5 + 0.5 * np.sin(2 * np.pi * 0.11 * t)) ** 4
        buf += 0.006 * crick
    return loop_track(bars * bar, render)


def sfx_click():
    n = int(0.06 * SR); t = np.arange(n) / SR
    tick = highpass(rng.standard_normal(n), 2500) * np.exp(-t * 180)
    thump = np.sin(2 * np.pi * 180 * t) * np.exp(-t * 70)
    return 0.5 * tick + 0.6 * thump


def sfx_page():
    n = int(0.32 * SR); t = np.arange(n) / SR
    noise = lowpass(highpass(rng.standard_normal(n), 900), 6000)
    e = np.sin(np.pi * np.clip(t / 0.30, 0, 1)) ** 2 * (1 + 0.6 * np.sin(2 * np.pi * 28 * t))
    return 0.5 * noise * e


def sfx_bell():
    s = bell(midi(84), 1.6) * 0.6
    place(s, bell(midi(88), 1.4) * 0.35, 0.0)
    shake = np.zeros_like(s)
    place(shake, bell(midi(84), 1.0) * 0.3, 0.09)
    return reverb(s + shake, 1.0, 0.25)


def sfx_pen():
    n = int(0.45 * SR); t = np.arange(n) / SR
    noise = highpass(rng.standard_normal(n), 3000)
    strokes = np.clip(np.sin(2 * np.pi * 9 * t), 0, 1) ** 0.6
    return 0.28 * noise * strokes * np.sin(np.pi * t / t[-1])


def sfx_drop():
    n = int(0.5 * SR); t = np.arange(n) / SR
    thud = np.sin(2 * np.pi * 90 * t * (1 - 0.3 * t)) * np.exp(-t * 14)
    rustle = lowpass(rng.standard_normal(n), 2500) * np.exp(-t * 20) * 0.3
    return 0.8 * thud + rustle


def write(name, x, peak=0.85):
    x = np.asarray(x, dtype=np.float64)
    x = x / (np.max(np.abs(x)) + 1e-9) * peak
    fade = int(0.003 * SR)
    if not name.startswith('music'):
        x[-fade:] *= np.linspace(1, 0, fade)
    with wave.open(os.path.join(OUT, name + '.wav'), 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype('<i2').tobytes())
    print(f'{name}.wav  {len(x) / SR:.1f}s')


if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True)
    write('music_day', music_day(), 0.7)
    write('music_night', music_night(), 0.7)
    write('sfx_click', sfx_click(), 0.6)
    write('sfx_page', sfx_page(), 0.7)
    write('sfx_bell', sfx_bell(), 0.75)
    write('sfx_pen', sfx_pen(), 0.6)
    write('sfx_drop', sfx_drop(), 0.8)
