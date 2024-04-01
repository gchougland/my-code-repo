'''
This is just a script I wrote to automate picking meal choices for my weekly meal plan.
It utilizes my notion databases to pick and add meals.
'''
import json
import requests
from datetime import datetime, timedelta
import random

NOTION_API_KEY = "" # Insert secret key here
# Always start on the next sunday or today if its sunday
date = datetime.today() + timedelta(days=6-datetime.today().weekday())
headers = {
  "Authorization": f'Bearer {NOTION_API_KEY}',
  "Notion-Version": '2022-06-28',
  "Content-Type": "application/json"
}
mealDatabaseID = "f507cbf441cd488eb1326953e8f69f8a"
mealPlansDatabaseID = "0dc09b87eb1d491b9c92396e15864d5a"


url = f"https://api.notion.com/v1/databases/{mealDatabaseID}/query"
data = {
  "filter": {
    "and": [
      {
        "property": "Course",
        "multi_select": {
          "contains": "Lunch"
        }
      },
      {
        "or": [
          {
            "property": "Rating",
            "select": {
              "equals": "★★★★★"
            }
          },
          {
            "property": "Rating",
            "select": {
              "equals": "★★★★"
            }
          },
          {
            "property": "Rating",
            "select": {
              "equals": "★★★"
            }
          }
        ]
      }
    ]
  }
}
response = requests.post(url, json=data, headers=headers)
print(json.dumps(response.json(), indent=4))
LunchOptions = []
for page in response.json()["results"]:
  LunchOptions.append({"id":page["id"]})
data["filter"]["and"][0]["multi_select"]["contains"] = "Dinner"
response = requests.post(url, json=data, headers=headers)
DinnerOptions = []
for page in response.json()["results"]:
  DinnerOptions.append({"id":page["id"]})

# Create Weekday Pages

url = "https://api.notion.com/v1/pages"
for i in range(6):
  weekday = date + timedelta(days=i)
  data = {
    "parent": {
          "database_id":mealPlansDatabaseID
    },
    "properties": {
        "Name": {
          "title": [
            {
              "text": {
                "content": f"{weekday.strftime('%A')} {weekday.strftime("%m-%d")}"
              }
            }
          ]
        },
        "Date": {
           "type": "date",
            "date": {
                "start": weekday.strftime("%Y-%m-%d")
            }
        },
        "Lunch": {
          "relation": [
            random.choice(LunchOptions)
          ]
        },
        "Dinner": {
          "relation": [
            random.choice(DinnerOptions)
          ]
        }
    }
  }
  response = requests.post(url, json=data, headers=headers)
#print(json.dumps(response.json(), indent=4))

'''
for page in response.json()["results"]:
    if page["properties"]["Date"]["date"]["start"] == "2024-04-01":
        url = f"https://api.notion.com/v1/pages/{page['id']}"
        data = {
            "properties": {
                "Notes": { 
                    "rich_text" : [
                      {
                        "type": "text",
                        "text": {
                          "content": "Hello world"
                        }
                      }
                    ]
                }
            }
        }
        newResponse = requests.patch(url, json=data, headers=headers)
        print(json.dumps(newResponse.json(), indent=4))

filterExample = {
  "filter": {
      "property": "Name",
      "rich_text": {
        "equals": "Monday"
      }
  },
  "sorts": [
    {
      "property": "Date",
      "direction": "descending"
    }
  ]
}

'''