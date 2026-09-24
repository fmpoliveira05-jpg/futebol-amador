import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateTeamDto } from '../../../shared/Dtos/Team/CreateTeamDto';
import { imagemQuadrada, srcEmblema } from '../../../shared/imagem/imagem';

/**
 * Formulário de uma equipa (nome, descrição, emblema e campo), usado para criar e para editar.
 * Os limites são os mesmos que a API valida (`ModelConstants`).
 */
@Component({
  selector: 'app-equipa-form',
  imports: [ReactiveFormsModule],
  templateUrl: './equipa-form.html',
  styleUrl: './equipa-form.css',
})
export class EquipaForm {
  /** Dados atuais da equipa, quando se está a editar. */
  readonly inicial = input<CreateTeamDto | null>(null);
  readonly aGuardar = input(false);
  readonly textoBotao = input('Guardar');
  readonly podeCancelar = input(false);

  readonly guardar = output<CreateTeamDto>();
  readonly cancelar = output<void>();

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    description: ['', [Validators.maxLength(250)]],
    pitchName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    pitchAddress: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(250)]],
  });

  /** Emblema escolhido nesta edição (data URL), ou `null` se não mudou. */
  protected readonly novoEmblema = signal<string | null>(null);
  protected readonly erroEmblema = signal<string | null>(null);
  protected readonly preview = signal(srcEmblema(null));

  constructor() {
    effect(() => {
      const equipa = this.inicial();
      if (equipa) {
        this.form.reset({
          name: equipa.name,
          description: equipa.description ?? '',
          pitchName: equipa.homePitch?.name ?? '',
          pitchAddress: equipa.homePitch?.address ?? '',
        });
      }
      this.novoEmblema.set(null);
      this.preview.set(srcEmblema(equipa?.icon));
    });
  }

  protected async escolherEmblema(evento: Event): Promise<void> {
    const ficheiro = (evento.target as HTMLInputElement).files?.[0];
    if (!ficheiro) {
      return;
    }
    this.erroEmblema.set(null);
    try {
      const dataUrl = await imagemQuadrada(ficheiro);
      this.novoEmblema.set(dataUrl);
      this.preview.set(dataUrl);
    } catch (e) {
      this.erroEmblema.set(e instanceof Error ? e.message : 'Não foi possível ler a imagem.');
    }
  }

  protected submeter(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.guardar.emit({
      name: v.name.trim(),
      description: v.description.trim(),
      // `null` mantém o emblema atual (a API só o substitui se vier preenchido).
      icon: this.novoEmblema(),
      homePitch: { name: v.pitchName.trim(), address: v.pitchAddress.trim() },
    });
  }

  protected invalido(campo: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[campo];
    return c.invalid && c.touched;
  }
}
