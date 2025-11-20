package com.example.amfootball.ui.viewModel

import com.google.firebase.Firebase
import com.google.firebase.auth.auth
import android.util.Log
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.chat.ChatRoom
import com.example.amfootball.data.dtos.chat.MessageDto
import com.google.firebase.firestore.FieldValue
import com.google.firebase.firestore.ListenerRegistration
import com.google.firebase.firestore.Query
import com.google.firebase.firestore.firestore
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow


class ChatViewModel : ViewModel() {
    private val db = Firebase.firestore
    private val myUserId = Firebase.auth.currentUser?.uid

    private val _rooms = MutableStateFlow<List<ChatRoom>>(emptyList())
    val rooms = _rooms.asStateFlow() // A UI vai observar isto

    private val _messages = MutableStateFlow<List<MessageDto>>(emptyList())
    val messages = _messages.asStateFlow() // A UI vai observar isto

    private var messagesListener: ListenerRegistration? = null

    fun fetchMyChatRooms(userId: String) {
        db.collection("chatRooms")
            .whereArrayContains("members", userId)
            .get()
            .addOnSuccessListener { querySnapshot ->
                _rooms.value = querySnapshot.toObjects(ChatRoom::class.java)
            }
            .addOnFailureListener { e ->
                Log.e("Chat", "Erro ao listar salas", e)
            }
    }

    fun listenForMessages(roomId: String) {

        messagesListener?.remove()

        val query = db.collection("chatRooms").document(roomId)
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
    fun sendMessage(roomId: String, messageText: String) {
        if (myUserId == null) return

        val message = hashMapOf(
            "text" to messageText,
            "senderId" to myUserId,
            "timestamp" to FieldValue.serverTimestamp()
        )

        db.collection("chatRooms").document(roomId)
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


    override fun onCleared() {
        super.onCleared()
        Log.d("ChatViewModel", "Limpando listener de mensagens.")
        messagesListener?.remove()
    }
}

