package com.example.amfootball.ui.viewModel.chat

import android.util.Log
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.chat.ChatRoom
import com.example.amfootball.data.dtos.chat.MessageDto
import com.example.amfootball.navigation.Objects.Routes
import com.google.firebase.Firebase
import com.google.firebase.auth.auth
import com.google.firebase.firestore.FieldValue
import com.google.firebase.firestore.ListenerRegistration
import com.google.firebase.firestore.Query
import com.google.firebase.firestore.firestore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

@HiltViewModel
class ChatViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,

    ): ViewModel() {
    private val db = Firebase.firestore
    private val myUserId = Firebase.auth.currentUser?.uid

    val chatRoomId: String? = savedStateHandle[Routes.chatRoomId]

    private val _rooms = MutableStateFlow<List<ChatRoom>>(emptyList())
    val rooms = _rooms.asStateFlow() // A UI vai observar isto

    private val _messages = MutableStateFlow<List<MessageDto>>(emptyList())
    val messages = _messages.asStateFlow() // A UI vai observar isto

    private var messagesListener: ListenerRegistration? = null

    init {
        if (myUserId != null) {
            fetchMyChatRooms()
        }
    }

    fun loadSingleChat(ChatRoomId: String) {

    }

    fun fetchMyChatRooms() {
        if (myUserId == null) return

        db.collection("chatRooms")
            .whereArrayContains("members", myUserId)
            .get()
            .addOnSuccessListener { querySnapshot ->
                _rooms.value = querySnapshot.toObjects(ChatRoom::class.java)
            }
            .addOnFailureListener { e ->
                Log.e("Chat", "Erro ao listar salas", e)
            }
    }

    fun listenForMessages() {
        if (chatRoomId == null){
            return
        }
        messagesListener?.remove()

        val query = db.collection("chatRooms").document(chatRoomId)
            .collection("messages")
            .orderBy("timestamp", Query.Direction.ASCENDING) // ASCENDING = mais antigo primeiro
            .limit(50)

        messagesListener = query.addSnapshotListener { snapshot, error ->
            if (error != null) {
                Log.w("Chat", "Erro ao ouvir mensagens.", error)
                return@addSnapshotListener
            }

            if (snapshot != null) {
                _messages.value = snapshot.toObjects(MessageDto::class.java)
            }
        }
    }
    fun sendMessage( messageText: String) {
        if (myUserId == null) return
        if (chatRoomId == null) return

        val message = hashMapOf(
            "text" to messageText,
            "senderId" to myUserId,
            "timestamp" to FieldValue.serverTimestamp()
        )

        db.collection("chatRooms").document(chatRoomId)
            .collection("messages")
            .add(message)
            .addOnSuccessListener {
                Log.d("Chat", "Mensagem enviada com sucesso!")
            }
            .addOnFailureListener { e ->
                Log.w("Chat", "Erro ao enviar mensagem", e)
            }
    }

    fun isSentByMe(message: MessageDto): Boolean {
        return message.senderId == myUserId
    }

    fun getChatRoomName(): String {
        for (room in _rooms.value) {
            if (room.id == chatRoomId) {
                return room.name
            }
        }
        return ""
    }


    override fun onCleared() {
        super.onCleared()
        Log.d("ChatViewModel", "Limpando listener de mensagens.")
        messagesListener?.remove()
    }
}