package com.example.amfootball.ui.viewModel.abstracts

import com.example.amfootball.data.network.NetworkConnectivityObserver
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow

/**
 * ViewModel Abstrato e Genérico para gestão de Ecrãs de Formulário.
 *
 * Esta classe serve de base para qualquer ecrã que envolva criação ou edição de dados (ex: Criar Equipa, Editar Perfil).
 * Centraliza a lógica repetitiva de:
 * 1. Gestão de Estado dos Dados ([uiFormState]).
 * 2. Gestão de Erros de Validação ([uiFormErrors]).
 * 3. Fluxo padrão de Submissão (Validar -> Verificar Net -> API -> Sucesso).
 * 4. Fluxo padrão de Carregamento de dados para edição.
 *
 * Herda de [BaseViewModel] para obter gestão de loading e conectividade.
 *
 * @param F O tipo de dados do formulário (Form Data DTO), ex: [FormTeamDto].
 * @param E O tipo de objeto de erros (Form Errors DTO), ex: [TeamFormErros].
 * @property networkObserver Observador de rede passado ao [BaseViewModel].
 * @param initialData O estado inicial dos dados (objeto vazio ou pré-preenchido).
 * @param initialError O estado inicial dos erros (objeto vazio).
 */
abstract class FormsViewModel<F, E>(
    private val networkObserver: NetworkConnectivityObserver,
    initialData: F,
    initialError: E,
) : BaseViewModel(networkObserver = networkObserver, needObserverNetwork = false) {
    /**
     * Propriedade mutável interna que guarda os dados atuais do formulário.
     * É [protected] para que as classes filhas possam atualizar os campos (via Setters).
     */
    protected val formState: MutableStateFlow<F> = MutableStateFlow(initialData)

    /**
     * Fluxo imutável e público dos dados do formulário.
     * Observado pela UI (Jetpack Compose) para preencher os campos de texto.
     */
    val uiFormState: StateFlow<F> = formState

    /**
     * Propriedade mutável interna que guarda os erros de validação atuais.
     * É [protected] para que as classes filhas possam definir erros no método [validateForm].
     * */
    protected val formErrors: MutableStateFlow<E> = MutableStateFlow(initialError)

    /**
     * Fluxo imutável e público dos erros.
     * Observado pela UI para mostrar mensagens vermelhas abaixo dos campos inválidos.
     */
    val uiFormErrors: StateFlow<E> = formErrors

    /**
     * Método abstrato de validação.
     *
     * Deve ser implementado por cada ViewModel filho para aplicar as regras de negócio específicas
     * (ex: tamanho de strings, campos obrigatórios, regex).
     *
     * @return `true` se o formulário for válido, `false` caso contrário.
     */
    protected abstract fun validateForm(): Boolean

    /**
     * Helper que executa o fluxo padrão de submissão de um formulário.
     *
     * Automatiza os seguintes passos:
     * 1. Chama [validateForm]. Se retornar `false`, interrompe o processo.
     * 2. Chama [launchDataLoad] (do [BaseViewModel]) para:
     * - Iniciar o Loading.
     * - Verificar se há Internet (bloqueia se estiver offline).
     * - Tratar exceções (Try-Catch) automaticamente.
     * 3. Executa a [apiCall] (chamada ao repositório definida pelo filho).
     * 4. Se tudo correr bem, executa o callback [onSuccess] (geralmente navegação).
     *
     * @param onSuccess Lambda executado após a API retornar sucesso (ex: Navegar para Home).
     * @param apiCall Lambda suspenso que contém a chamada ao Repositório (Create ou Update).
     */
    protected fun submitForm(onSuccess: () -> Unit = {}, apiCall: suspend () -> Unit) {
        if (!validateForm()) return

        launchDataLoad {
            apiCall()

            onSuccess()
        }
    }

    /**
     * Helper para carregar dados iniciais em modo de Edição.
     *
     * Executa a chamada à API fornecida em [block] e atualiza automaticamente
     * o [formState] com o resultado obtido.
     *
     * @param block Lambda suspenso que chama o repositório e retorna o objeto de dados [F].
     */
    protected fun loadFormData(block: suspend () -> F) {
        launchDataLoad {
            val result = block()
            formState.value = result
        }
    }
}