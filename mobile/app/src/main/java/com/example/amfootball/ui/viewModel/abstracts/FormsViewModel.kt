package com.example.amfootball.ui.viewModel.abstracts

import com.example.amfootball.data.NetworkConnectivityObserver
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

/**
 * **ViewModel Abstrato e Genérico para Gestão de Formulários.**
 *
 * Esta classe atua como um componente arquitetural base para todos os ecrãs que envolvem a recolha
 * e submissão de dados (ex: Criar Equipa, Editar Perfil, Registar Jogador).
 *
 * **Principais Responsabilidades:**
 * 1.  **Centralização de Boilerplate:** Remove a necessidade de repetir a lógica de `StateFlows` e validações em cada ViewModel.
 * 2.  **Segurança de Tipos (Generics):** Garante que o estado do formulário [F] e os erros [E] estão estritamente tipados.
 * 3.  **Fluxo de Submissão Padronizado:** Implementa o padrão "Template Method" no [submitForm], forçando a ordem: Validação -> Verificação de Rede -> Loading -> API -> Sucesso.
 * 4.  **Gestão de Erros:** Integra-se com o [BaseViewModel] para capturar exceções de rede e erros desconhecidos automaticamente.
 *
 * **Herança:**
 * Herda de [BaseViewModel] para aproveitar a gestão de:
 * - `isLoading` (Estado de carregamento).
 * - `errorMessage` / `toastMessage` (Feedback ao utilizador).
 * - Conectividade de rede.
 *
 * @param F **(Form Data)**: O tipo de dados que representa o estado visual dos campos. Geralmente um Data Class (ex: `CreateTeamDto`).
 * @param E **(Form Errors)**: O tipo de dados que representa as mensagens de erro de validação. Geralmente um Data Class com campos Nullable (ex: `TeamFormErrors`).
 * @param networkObserver Observador de conectividade injetado para verificar o estado da internet antes de submissões.
 * @param initialData O estado inicial do formulário. Pode ser um objeto vazio (para criação) ou pré-preenchido (para edição).
 * @param initialError O estado inicial dos erros (geralmente um objeto com todos os campos a `null`).
 */
abstract class FormsViewModel<F, E>(
    private val networkObserver: NetworkConnectivityObserver,
    private val needObserverNetwork: Boolean = true,
    initialData: F,
    initialError: E,
) : BaseViewModel(networkObserver = networkObserver, needObserverNetwork = needObserverNetwork) {

    /**
     * **Estado Interno Mutável dos Dados.**
     *
     * Guarda os valores atuais de cada campo do formulário.
     * É marcado como `protected` para permitir que as classes filhas (ViewModels concretos)
     * atualizem o estado através de métodos como `onNameChange`, `onEmailChange`, etc.
     */
    protected val formState: MutableStateFlow<F> = MutableStateFlow(initialData)

    /**
     * **Estado Público Imutável dos Dados.**
     *
     * Exposto para a UI (Jetpack Compose). Deve ser consumido utilizando `collectAsStateWithLifecycle()`.
     * Garante o princípio de "Unidirectional Data Flow", impedindo que a UI modifique o estado diretamente.
     */
    val uiFormState: StateFlow<F> = formState.asStateFlow()

    /**
     * **Estado Interno Mutável dos Erros.**
     *
     * Guarda as mensagens de erro atuais para cada campo.
     * Deve ser atualizado principalmente dentro do método [validateForm].
     */
    protected val formErrors: MutableStateFlow<E> = MutableStateFlow(initialError)

    /**
     * **Estado Público Imutável dos Erros.**
     *
     * Exposto para a UI. Permite que os componentes visuais (ex: `OutlinedTextField`) mostrem
     * mensagens de erro (texto vermelho) e estados de erro (bordas vermelhas) reativamente.
     */
    val uiFormErrors: StateFlow<E> = formErrors.asStateFlow()

    /**
     * **Lógica de Validação Abstrata.**
     *
     * Este método é o "coração" da validação. Cada ViewModel concreto deve implementá-lo
     * com as suas regras de negócio específicas (ex: "nome não pode ser vazio", "email deve conter @").
     *
     * **Contrato de Implementação:**
     * 1. Verificar cada campo do [formState].
     * 2. Se um campo for inválido, atualizar o [formErrors] com a mensagem correspondente.
     * 3. Se um campo for válido, garantir que o erro correspondente em [formErrors] é limpo (`null`).
     *
     * @return `true` se todos os dados estiverem válidos e prontos para envio; `false` caso contrário.
     */
    protected abstract fun validateForm(): Boolean

    /**
     * **Orquestrador de Submissão de Formulário.**
     *
     * Este método encapsula todo o ciclo de vida de uma operação de escrita (POST/PUT/PATCH).
     *
     * **Fluxo de Execução:**
     * 1. **Validação:** Executa [validateForm]. Se falhar (retornar `false`), o processo para imediatamente e os erros são mostrados na UI.
     * 2. **Corrotina:** Lança uma corrotina segura via [launchDataLoad] (do [BaseViewModel]).
     * 3. **Conectividade:** O [launchDataLoad] verifica automaticamente se há internet. Se não houver, exibe um Toast e aborta.
     * 4. **Loading:** O estado `isLoading` passa a `true` (bloqueando botões na UI, se configurado).
     * 5. **API Call:** Executa o bloco [apiCall] suspenso fornecido.
     * 6. **Sucesso:** Se a API não lançar exceções, o callback [onSuccess] é invocado (ideal para navegação ou limpeza).
     * 7. **Erro:** Se a API falhar, o [BaseViewModel] captura a exceção, atualiza o `errorMessage` e esconde o loading.
     *
     * @param onSuccess Callback executado apenas se a chamada à API for bem-sucedida.
     * @param apiCall Bloco suspenso que contém a lógica de rede (ex: `repository.createTeam(dto)`).
     */
    protected fun submitForm(onSuccess: () -> Unit = {}, apiCall: suspend () -> Unit) {
        if (!validateForm()) return

        launchDataLoad {
            apiCall()

            onSuccess()
        }
    }

    /**
     * **Carregamento de Dados para Edição.**
     *
     * Método utilitário para cenários de "Update/Edit".
     * Realiza uma chamada à API para obter os dados existentes e preenche o [formState] automaticamente.
     *
     * Também utiliza o [launchDataLoad] para gerir loadings e erros durante a busca (GET).
     *
     * @param block Bloco suspenso que retorna o objeto de dados [F] vindo do repositório/API.
     */
    protected fun loadFormData(block: suspend () -> F) {
        launchDataLoad {
            val result = block()
            formState.value = result
        }
    }
}