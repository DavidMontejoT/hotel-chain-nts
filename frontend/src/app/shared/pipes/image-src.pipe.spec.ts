import { environment } from '../../../environments/environment';
import { ImageSrcPipe } from './image-src.pipe';

describe('ImageSrcPipe', () => {
  const pipe = new ImageSrcPipe();

  it('sirve las imágenes subidas desde el origen de la API', () => {
    const origin = environment.apiUrl.replace(/\/api\/?$/, '');
    expect(pipe.transform('/uploads/abc.png')).toBe(`${origin}/uploads/abc.png`);
  });

  it('deja pasar las URLs externas y los valores vacíos', () => {
    expect(pipe.transform('https://example.com/a.jpg')).toBe('https://example.com/a.jpg');
    expect(pipe.transform(null)).toBeNull();
  });
});
