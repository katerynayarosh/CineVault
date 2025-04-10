namespace CineVault.API.Entities;

// TODO 5 ��������� ������ ���� ������� Actor, ��� ���� ������������ ������ � ��� �����. ������� ������ ���� ���� FullName, BirthDate, Biography. ���������� ������ ���� "������ �� ��������" �� ������� Movie (�����) �� ����� ������� Actor.
public sealed class Actor
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? Biography { get; set; }
    public ICollection<Movie> Movies { get; set; } = [];
    // TODO 10 ����������� �������� "�'����� ���������" (Soft Delete) ��� ���������. ������ ���� IsDeleted (property) �� ����� �������
    public bool IsDeleted { get; set; }
}