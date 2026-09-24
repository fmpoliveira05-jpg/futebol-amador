/** Imagem mostrada quando a equipa não tem emblema. */
export const EMBLEMA_PREDEFINIDO = 'assets/placeholder.png';

/** Tamanho máximo aceite antes de redimensionar (evita ler ficheiros enormes para memória). */
export const TAMANHO_MAXIMO_IMAGEM = 10 * 1024 * 1024;

/**
 * Devolve um `src` válido para o emblema de uma equipa.
 *
 * O emblema pode vir como URL (a app Android guarda-o no Cloudinary), como data URL (a web) ou,
 * em equipas antigas criadas pela web, como base64 sem prefixo.
 */
export function srcEmblema(icone: string | null | undefined): string {
  if (!icone) {
    return EMBLEMA_PREDEFINIDO;
  }
  if (/^(https?:|data:image\/)/.test(icone)) {
    return icone;
  }
  return `data:image/png;base64,${icone}`;
}

/**
 * Lê uma imagem escolhida pelo utilizador e reduz-a a um quadrado de `lado` píxeis (recorte
 * central), devolvida como data URL. Assim o emblema ocupa poucos KB na base de dados.
 */
export async function imagemQuadrada(ficheiro: File, lado = 256): Promise<string> {
  if (!ficheiro.type.startsWith('image/')) {
    throw new Error('O ficheiro escolhido não é uma imagem.');
  }
  if (ficheiro.size > TAMANHO_MAXIMO_IMAGEM) {
    throw new Error('A imagem é demasiado grande (máximo 10 MB).');
  }

  const bitmap = await createImageBitmap(ficheiro);
  const recorte = Math.min(bitmap.width, bitmap.height);
  const canvas = document.createElement('canvas');
  canvas.width = lado;
  canvas.height = lado;
  const ctx = canvas.getContext('2d');
  if (!ctx) {
    throw new Error('Não foi possível processar a imagem.');
  }
  ctx.drawImage(
    bitmap,
    (bitmap.width - recorte) / 2,
    (bitmap.height - recorte) / 2,
    recorte,
    recorte,
    0,
    0,
    lado,
    lado
  );
  bitmap.close();
  // Os browsers que não sabem gerar WebP devolvem PNG.
  return canvas.toDataURL('image/webp', 0.85);
}
