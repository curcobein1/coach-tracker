export interface WeightParseResult {
  ok: boolean;
  kg?: number;
  error?: string;
}

/**
 * Allowed examples:
 *  - "80"
 *  - "80.5"
 *  - "40+bar"
 *  - "bar+40"
 *  - "20+20+bar"
 *
 * Disallowed:
 *  - commas: "80,5"
 *  - any non-ASCII characters: ş ğ ü etc.
 *  - any other words besides "bar"
 */
export function parseKgInput(inputRaw: string, barKg = 20): WeightParseResult {
  const raw = (inputRaw ?? '').trim();

  if (!raw) return { ok: false, error: 'Enter weight' };

  // English-only: ASCII characters only
  for (const ch of raw) {
    const code = ch.charCodeAt(0);
    if (code > 127) return { ok: false, error: 'English-only input (ASCII). No ş/ğ/ü/etc.' };
  }

  const input = raw.toLowerCase();

  if (input.includes(',')) return { ok: false, error: 'Use "." for decimals (e.g., 80.5), not ","' };

  // Only allow digits/dot/plus/spaces/letters (letters only for "bar")
  if (!/^[0-9.+ a-z]+$/.test(input)) return { ok: false, error: 'Invalid characters' };

  const cleaned = input.replace(/\s+/g, '');

  // Split by plus and validate each token
  const parts = cleaned.split('+').filter(Boolean);
  if (parts.length === 0) return { ok: false, error: 'Invalid weight' };

  let total = 0;

  for (const p of parts) {
    if (p === 'bar') {
      total += barKg;
      continue;
    }
    // Reject any other words
    if (/[a-z]/.test(p)) return { ok: false, error: 'Only the word "bar" is allowed' };

    if (!/^\d+(\.\d+)?$/.test(p)) return { ok: false, error: `Invalid number: "${p}"` };
    total += Number(p);
  }

  if (!Number.isFinite(total) || total <= 0) return { ok: false, error: 'Weight must be > 0' };
  return { ok: true, kg: Math.round(total * 10) / 10 };
}
