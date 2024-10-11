using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sharprompt;
using ConsoleTables;
using System.ComponentModel.DataAnnotations;

namespace Timetable
{
    public class MainTimetable
    {
        // Player can choose from a selection of courses.
        // Some three units, some two uints.
        // Create the timetable array first.

        struct Course {
            public readonly string name;
            public readonly bool isThreeUnits;

            List<(int row, int column)> spotOnTimetable = new List<(int, int)>();

            public Course(string _name, bool _isThreeUnits) {
                name = _name;
                isThreeUnits = _isThreeUnits;
            }

            public void addSpotToTimeTable((int row, int column) spot) {
                 spotOnTimetable.Add((spot.row, spot.column));
            }

            public int getRowForTwoHourClass() {
                if(spotOnTimetable.Count == 0) {
                    return -1;
                }

                return spotOnTimetable[0].row;
            }
        }

        String[,] timeTable = new String[5, 9]; //first is row, second is colum

        List<Course> courses = new List<Course>();

        string[] threeUnitsCourseList = ["English", "Maths", "Physics", "Biology", "Chemisty"];

        List<string> twoUnitsCourseList = ["Accounting", "Economics", "Computer", "Agric"];

        public void Start() {
            PopulateTimeTable();
            PromptUser();
            CreateTimetable();

            Console.WriteLine("Enter any key to close");
            Console.ReadKey();
        }
        
        public void PromptUser() {
            string prompt = "Select three courses only";

            Console.WriteLine("--- THREE UNIT COURSES ---");
            var selectedThreeUnits = Prompt.MultiSelect<string>(prompt, threeUnitsCourseList, minimum: 3, maximum:3);

            Console.WriteLine("--- TWO UNIT COURSES ---");
            var selectedTwoUnits = Prompt.MultiSelect<string>(prompt, twoUnitsCourseList, minimum: 3, maximum:3);

            foreach (var item in selectedThreeUnits)
            {
                courses.Add(new Course(item.ToString(), true));
            }

            foreach (var item in selectedTwoUnits)
            {
                courses.Add(new Course(item.ToString(), false));
            }
        }

        public void PopulateTimeTable() {
            int x = 1;
            for(int i = 0; i < timeTable.GetLength(0); i++) {
                for(int j = 0; j < timeTable.GetLength(1); j++) {
                    timeTable[i, j] = x.ToString();
                    x++;
                }
            }
        }

        public void CreateTimetable() {
            foreach (Course course in courses)
            {
                if(course.isThreeUnits) {
                    int spot = setTwoHoursOnTimeTable(course);
                    course.addSpotToTimeTable(getRowAndColumn(spot));
                    course.addSpotToTimeTable(getRowAndColumn(spot + 1));

                    spot = setOneHourOnTimeTable(course);
                    course.addSpotToTimeTable(getRowAndColumn(spot));
                } else if(!course.isThreeUnits) {
                    int spot = setOneHourOnTimeTable(course);
                    course.addSpotToTimeTable(getRowAndColumn(spot));
                }
            }

            DisplayTimetable();
        }

        (int, int) getRowAndColumn(int spot) {
            int row = ((int) (Math.Ceiling((double)spot/9))) - 1;

            /* Subtract till a negative value. Add 9 to it.
            If it gives 0 then it's on the 9th row */
            int column = spot;
            while(column > 0) { column -= 9; }

            column += 9;
            if(column == 0) { column = 9; }

            column -= 1;
            return (row, column);
        }

        bool isSpotFree(int spot) {
            if(spot > 45 || spot < 1) {
                return false;
            }

            (int row, int column) = getRowAndColumn(spot);
            return timeTable[row, column].Any(char.IsDigit);
        }

        int setOneHourOnTimeTable(Course course) {
            Random random = new Random();
            int spot = random.Next(1, 46);
            (int row, int column) = getRowAndColumn(spot);

            while(isSpotFree(spot) == false || row == course.getRowForTwoHourClass() || spot % 9 == 5) {
                spot = random.Next(1, 46);
                (row, column) = getRowAndColumn(spot);
            }

            // found a free spot
            timeTable[row, column] = course.name;
            return spot;
        } 

        int setTwoHoursOnTimeTable(Course course) {
            Random random = new Random();
            int spot = random.Next(1, 46);
            int nextSpot = spot + 1;

            while(isSpotFree(spot) == false || isSpotFree(nextSpot) == false || spot % 9 == 0 || spot % 9 == 5 || nextSpot % 9 == 5) {
                spot = random.Next(1, 46);
                nextSpot = spot + 1;
            }

            // found a free spot
            (int row, int column) = getRowAndColumn(spot);
            timeTable[row, column] = course.name;

            (int nextRow, int nextColumn) = getRowAndColumn(nextSpot);
            timeTable[nextRow, nextColumn] = course.name;

            return spot;
        }

        private string GetDay(int i) {
            string name = "null";
            switch (i)
            {
                case 0:
                    name = "Monday";
                    break;
                case 1:
                    name = "Tuesday";
                    break;
                case 2:
                    name = "Wednesday";
                    break;
                case 3:
                    name = "Thursday";
                    break;
                case 4:
                    name = "Friday";
                    break;
                default:
                    name = "error";
                    break;
            }

            return name;
        }

        public void DisplayTimetable() {
            string[] timeSlots = new string[9];
            ConsoleTable table = new ConsoleTable("", "8am", "9am", "10am", "11am", "12am", "1pm", "2pm", "3pm", "4pm");

            for(int i = 0; i < timeTable.GetLength(0); i++) {
                for(int j = 0; j < timeTable.GetLength(1); j++) {
                    string slot = timeTable[i, j];
                    if(slot.Any(char.IsDigit) == true) {
                        slot = "";
                    }
                    if(j == 4) {
                        slot = "BREAK";
                    }
                    timeSlots[j] = slot;
                }

                table.AddRow(GetDay(i), timeSlots[0], timeSlots[1], timeSlots[2], timeSlots[3], timeSlots[4], timeSlots[5], timeSlots[6], timeSlots[7], timeSlots[8]);
            }

            table.Write();
        }
    }
}