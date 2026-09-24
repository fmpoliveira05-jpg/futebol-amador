import { FormControl } from '@angular/forms';
import { idadeMinima } from './signup.component';

describe('idadeMinima', () => {
  const anosAtras = (anos: number, dias = 0) => {
    const d = new Date();
    d.setFullYear(d.getFullYear() - anos);
    d.setDate(d.getDate() + dias);
    return d.toISOString().slice(0, 10);
  };

  it('aceita quem tem 18 anos ou mais', () => {
    expect(idadeMinima(new FormControl(anosAtras(18, -1), { nonNullable: true }))).toBeNull();
    expect(idadeMinima(new FormControl(anosAtras(40), { nonNullable: true }))).toBeNull();
  });

  it('recusa quem ainda não fez 18 anos', () => {
    expect(idadeMinima(new FormControl(anosAtras(18, 2), { nonNullable: true }))).toEqual({ idadeMinima: true });
  });

  it('deixa o campo vazio para o validador required', () => {
    expect(idadeMinima(new FormControl('', { nonNullable: true }))).toBeNull();
  });
});
