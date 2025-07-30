using UnityEngine;
using System.Collections;
public class MoveBetweenGoals : MonoBehaviour
{
    // Компонент агента
    UnityEngine.AI.NavMeshAgent agent;
    // Массив возможных точек назначения
    public Transform[] goals;
    // Расстояние на котором происходит переключение к точке
    public float distanceToChangeGoal;
    // Номер текущей целевой точки
    int currentGoal = 0;
    void Start()
    {
        // Получение компонента агента и направление к первой точке
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = goals[0].position;
    }
    void Update()
    {
        // Проверка на то, достаточно ли близко агент к цели
        if (agent.remainingDistance < distanceToChangeGoal)
        {
            // Смена точки на следующую
            currentGoal++;
            if (currentGoal == goals.Length) currentGoal = 0;
            agent.destination = goals[currentGoal].position;
        }
    }
}