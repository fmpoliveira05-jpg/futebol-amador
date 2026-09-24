import { EMBLEMA_PREDEFINIDO, imagemQuadrada, srcEmblema } from './imagem';

describe('srcEmblema', () => {
  it('usa a imagem predefinida quando não há emblema', () => {
    expect(srcEmblema(null)).toBe(EMBLEMA_PREDEFINIDO);
    expect(srcEmblema('')).toBe(EMBLEMA_PREDEFINIDO);
  });

  it('mantém URLs e data URLs', () => {
    expect(srcEmblema('https://res.cloudinary.com/x.png')).toBe('https://res.cloudinary.com/x.png');
    expect(srcEmblema('data:image/png;base64,AAAA')).toBe('data:image/png;base64,AAAA');
  });

  it('acrescenta o prefixo ao base64 antigo', () => {
    expect(srcEmblema('iVBORw0KGgo')).toBe('data:image/png;base64,iVBORw0KGgo');
  });
});

describe('imagemQuadrada', () => {
  it('recusa ficheiros que não são imagens', async () => {
    const ficheiro = new File(['texto'], 'a.txt', { type: 'text/plain' });
    await expectAsync(imagemQuadrada(ficheiro)).toBeRejectedWithError(/não é uma imagem/);
  });

  it('reduz uma imagem a um quadrado', async () => {
    const canvas = document.createElement('canvas');
    canvas.width = 400;
    canvas.height = 200;
    canvas.getContext('2d')!.fillRect(0, 0, 400, 200);
    const blob = await new Promise<Blob>((r) => canvas.toBlob((b) => r(b!), 'image/png'));
    const dataUrl = await imagemQuadrada(new File([blob], 'emblema.png', { type: 'image/png' }), 64);

    const img = new Image();
    img.src = dataUrl;
    await img.decode();
    expect(img.width).toBe(64);
    expect(img.height).toBe(64);
  });
});
